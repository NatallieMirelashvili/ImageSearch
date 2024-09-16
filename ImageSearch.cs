using System;
using System.Threading;
using System.Drawing;
using System.IO;
using static System.Net.Mime.MediaTypeNames;




public class ImageSearch {

    //decode the images step 1

    public static byte[] ImagePathToByteArray(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            Console.WriteLine($"The file {imagePath} does not exist.");
        }
        return File.ReadAllBytes(imagePath);
    }

    //decode the images step 2
    public static Bitmap BytesToBitmap(byte[] imageBytes)
    {
        try
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                return new Bitmap(ms);
            }
        }
        catch (ArgumentException e)
        {
            throw new InvalidOperationException("Failed to convert byte array to Bitmap. The byte array may be invalid.", e);
        }
    }

    //decode the images step 3
    public static Color[][] BitmapToColorMatrix(Bitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;
        Color[][] colorMatrix = new Color[height][];

        for (int y = 0; y < height; y++)
        {
            colorMatrix[y] = new Color[width];
            for (int x = 0; x < width; x++)
            {
                colorMatrix[y][x] = bitmap.GetPixel(x, y);
            }
        }

        return colorMatrix;
    }

    public static double CalculateEuclideanDistance(Color c1, Color c2)
    {
        int r1 = c1.R, g1 = c1.G, b1 = c1.B;
        int r2 = c2.R, g2 = c2.G, b2 = c2.B;

        return Math.Sqrt((r1 - r2) * (r1 - r2) + (g1 - g2) * (g1 - g2) + (b1 - b2) * (b1 - b2));
    }

    public static bool compareRestPic(Color[][] bigPic, Color[][] smallPic, int i, int j, string byAlgorithem) {
        if (byAlgorithem == "exact")
        {
            for (int x = 0; x < smallPic.Length ; x++)
            {
                for (int y = 0; y < smallPic[0].Length - 1; y++)
                {
                    if (!smallPic[x][y].Equals(bigPic[i + x][j + y]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        else if (byAlgorithem == "histogram")
        {
            double totalDistance = 0;
            for (int x = 0; x < smallPic.Length; x++)
            {
                for (int y = 0; y < smallPic[0].Length; y++)
                {
                    totalDistance += CalculateEuclideanDistance(bigPic[x+i][y+j], smallPic[x][y]);
                    if (totalDistance != 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        else 
            return false;
    }


    //int[] = (X,Y) or int[] if not found
    public static int[] byAlg(Color[][] bigPic, Color[][] smallPic, int startY,  int endY,string byAlgorithem) {
        int smallWidth = smallPic[0].Length;
        int smallHeight = smallPic.Length;
        int bigWidth = bigPic[0].Length;
        int bigHeight = bigPic.Length;
        int[] result = new int[2];
        int i;
        int j;
        int k = 0;
        int t = 0;
        for (i = 0; i < bigHeight - smallHeight + 1; i++)
        {
            for (j = startY; j < endY - smallWidth + 1; j++)
            {  
                Boolean check = compareRestPic(bigPic, smallPic, i, j, byAlgorithem);
                if (check == true) {
                    result[0] = i;
                    result[1] = j;
                    return result;
                    }
            }
        }
        return null;
    }



    public static void Main(string[] args)
    {
        if (args.Length < 4) {
            Console.WriteLine($"Please provide all the asked paramater to run the program!");
            return;
        }
        int numOfThreads = int.Parse(args[2]);
        if (numOfThreads < 1) {
            Console.WriteLine($"The number of thread to use you provide: {numOfThreads} smaller than 1");
            return;
        }
        string wantedAlgorithm = args[3];
        if (!(wantedAlgorithm.Equals("exact") || wantedAlgorithm.Equals("histogram")))
        {
            Console.WriteLine($"The name of algotithm you provide: {wantedAlgorithm} is not valid");
            return;
        }
        byte[] bigPic = ImagePathToByteArray(args[0]);
        byte[] smallPic = ImagePathToByteArray(args[1]);
        Bitmap bigPicBitmap = BytesToBitmap(bigPic);
        Bitmap smallPicBitmap  = BytesToBitmap(smallPic);
        Color[][] bigPicMat = BitmapToColorMatrix(bigPicBitmap);
        Color[][] smallPicMat = BitmapToColorMatrix(smallPicBitmap);
        int[][] resultsFromthread = new int[numOfThreads][]; 
        Thread[] threads = new Thread[numOfThreads];
        int jump = bigPicMat[0].Length / numOfThreads;
        for (int i = 0; i < numOfThreads; i++)
        {
            int startY = i * jump;
            int endY = startY + jump + smallPicMat[0].Length - 1;
            int threadIndex = i;
            if (endY > bigPicMat[0].Length)
            {
                endY = bigPicMat[0].Length;
            }
            threads[i] = new Thread(() => resultsFromthread[threadIndex] = byAlg(bigPicMat, smallPicMat, startY, endY, wantedAlgorithm));
            threads[i].Start();
        }
        for(int i = 0; i < numOfThreads; i++)
        {
            threads[i].Join();
        }
        for (int i = 0; i < numOfThreads; i++)
        {
            if (resultsFromthread[i] != null) {

                Console.WriteLine($"{resultsFromthread[i][0]},{resultsFromthread[i][1]}");
                return;
            }
        }
        Console.WriteLine("Not found");
        return;
    }


}
