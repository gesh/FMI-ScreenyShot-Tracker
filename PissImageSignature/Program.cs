using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

public class Program
{
    static void Main(string[] args)
    {
        var path = @"C:\Users\georgis.kukuzikis\Desktop\top.JPG";
        using (var fs = File.OpenRead(path))
        {
            AddWatermark(fs, "AAAA");
        }
    }

    public static void AddWatermark(FileStream fs, string watermarkText)
    {
        Image img = Image.FromStream(fs);
        Font font = new Font("Verdana", 50, FontStyle.Bold, GraphicsUnit.Pixel);
        //Adds a transparent watermark with an 100 alpha value.
        Color color = Color.FromArgb(255, 255, 255, 100);
        //The position where to draw the watermark on the image
        Point pt = new Point(10, 30);
        SolidBrush sbrush = new SolidBrush(color);

        Graphics gr = null;
        try
        {
            gr = Graphics.FromImage(img);
        }
        catch
        {
            // http://support.microsoft.com/Default.aspx?id=814675
            Image img1 = img;
            img = new Bitmap(img.Width, img.Height);
            gr = Graphics.FromImage(img);
            gr.DrawImage(img1, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
            img1.Dispose();
        }

        gr.DrawString(watermarkText, font, sbrush, pt);
        gr.Dispose();
        
        img.Save(@"C:\Users\georgis.kukuzikis\Desktop\top22.JPG");
        //img.Save(outputStream, ImageFormat.Jpeg);
    }

}

