using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;

namespace SplitAndMergeImages
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please Enter the URL of the Image:");
            string URL = Console.ReadLine();

            if(String.IsNullOrWhiteSpace(URL)) //If Url is not entered, Default it to following image
            {
                URL = @"https://images.pexels.com/photos/226589/pexels-photo-226589.jpeg";
            }

            Bitmap originalImage;
            try
            {
                using (var wc = new WebClient())
                {
                    byte[] bytes = wc.DownloadData(URL);
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        originalImage = new Bitmap(Image.FromStream(ms));
                    }
                }

            }
            catch (Exception)
            {
                throw new FileNotFoundException("Unable to download the image file");
            }

            Console.WriteLine("In how many images do you want to split the original image?");
            int splitCount = Int32.Parse(Console.ReadLine()); //No. of images in which the original image will be split

            //splitCount should be >15 as mentioned in the requirement.
            if (splitCount <= 15)
            {
                throw new ArgumentException("Split count should be greater than 15");
            }

            //Create the directory to output the split images
            Directory.CreateDirectory(@"C:\Split&MergerOutput\SplitImages\");
            string SplitOutputPath = @"C:\Split&MergerOutput\SplitImages\";

            //Call to Split() function to split original image into n images
            Split(SplitOutputPath, originalImage, splitCount);
            originalImage.Dispose();

            Console.WriteLine("Enter 1 to Merge split images");
            if (Console.ReadLine() == "1")
                Merge(SplitOutputPath); //Call to merge all the split images back to form the original image

            Console.WriteLine("Processing Complete. Thanks for using the Service");
        }

        //Split image into n images
        public static void Split(string splitOutputPath, Bitmap originalImage,int splitCount)
        {
            int SplitImagesWidth = originalImage.Width / splitCount; //width of n-1 images. The width of nth image will be SplitImagesWidth + originalImage.Width % splitCount
            int LeftOverImageWidth = originalImage.Width % splitCount;

            int i = 0, f = 1;
            try
            {
                while (i + LeftOverImageWidth < originalImage.Width)
                {
                    if (i + LeftOverImageWidth == originalImage.Width)
                        i += LeftOverImageWidth;

                    Rectangle rect = new Rectangle(i, 0, SplitImagesWidth, originalImage.Height);
                    Bitmap SplitImage = originalImage.Clone(rect, originalImage.PixelFormat);

                    string filename = splitOutputPath + f++ + ".jpg";
                    SplitImage.Save(filename, ImageFormat.Jpeg);

                    i += SplitImagesWidth;
                }

            }
            catch (Exception)
            {

                throw new Exception("Oops! Something went wrong while splitting the image.");
            }


            Console.WriteLine("Split successful. Split Images are available at following path:\n" + splitOutputPath);
        }

        //Merge images back to original image
        public static void Merge(string SplitOutputPath)
        {
            //Create the directory to output the Merged image
            Directory.CreateDirectory(@"C:\Split&MergerOutput\MergedImage\");
            string MergeOutputPath = @"C:\Split&MergerOutput\MergedImage\";

            //Get file info and sort as per creation time to work around the issue because of numeric sorting in string.
            DirectoryInfo di = new DirectoryInfo(SplitOutputPath);
            FileSystemInfo[] files = di.GetFileSystemInfos("*.jpg");
            var filePaths = files.OrderBy(f => f.CreationTime);

            int TotalWidth = 0;
            int MaxHeight = 0;
            List<Bitmap> imageList = new List<Bitmap>();
            
            //Get the total dimension of the final image from the input images
            foreach (var path in filePaths)
            {
                Bitmap image = new Bitmap(Image.FromFile(path.FullName));
                imageList.Add(image);
                TotalWidth += image.Width;
                if (image.Height > MaxHeight)
                    MaxHeight = image.Height;
            }

            //create a blank image of final output dimension
            using (Bitmap bitmap = new Bitmap(TotalWidth, MaxHeight))
            {

                //place all the images one by one as per the width and sequence on the above image.
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    int previousWidth = 0;
                    foreach (var image in imageList)
                    {

                        g.DrawImage(image, previousWidth, 0);

                        previousWidth += image.Width;
                        //g.DrawImage(secondHalf, firstHalf.Width, 0);
                    }
                }

                bitmap.Save(string.Format("{0}ReMerged.jpg", MergeOutputPath), ImageFormat.Jpeg);
            }

                Console.WriteLine("Image ReMerged. Please find the image at following Path\n" + MergeOutputPath + "ReMerged.jpg");
            }
    }
}
