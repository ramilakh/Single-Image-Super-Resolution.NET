/*
 * Created by SharpDevelop.
 * User: Ramil
 * Date: 08.04.2014
 * Time: 0:08
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace Single_Image_Super_Resolution.Kernel
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		int patchSize = 9;
		int k = 9/2, kk = 5;
		double ScaleConst = 1.2;
		
		void Button1Click(object sender, EventArgs e)
		{
			Kernel();
		}
		
		void Kernel()
		{
			Bitmap vImage = new Bitmap(@"pic.png", true);
			Bitmap vImageDest = new Bitmap(vImage, (int)(vImage.Width*ScaleConst), (int)(vImage.Height*ScaleConst));
			
			byte[,] vInImageArray = new byte[vImage.Height,vImage.Width];
			CodeYIQ(vImage, vInImageArray);
						
			List<byte[,]> SampledImages = new List<byte[,]>();
			Bitmap vImageI = vImage;
			double SampleConst = 1.2;
			for(int i = 0; i < 1/*3*/; i++)	//!!!
			{
				vImageI = new Bitmap(vImageI, (int)(vImageI.Width/SampleConst), (int)(vImageI.Height/SampleConst));
				
				byte[,] vInImageArrayI = new byte[vImageI.Height,vImageI.Width];
				CodeYIQ(vImageI, vInImageArrayI);
				SampledImages.Add(vInImageArrayI);
			}
			
			//decode to bmp in
			BitmapData vbmpDataIn = vImage.LockBits(new Rectangle(0,0,vImage.Width,vImage.Height),
                 System.Drawing.Imaging.ImageLockMode.ReadWrite, vImage.PixelFormat);
			byte[] vImageDataIn = new byte[vbmpDataIn.Stride * vbmpDataIn.Height];
			    System.Runtime.InteropServices.Marshal.Copy(vbmpDataIn.Scan0,vImageDataIn,0
			                                            ,vImageDataIn.Length);
			vImage.UnlockBits(vbmpDataIn);
			
			//decode to bmp out
			Bitmap vImageOut = new Bitmap(vImage, (int)(vImage.Width*ScaleConst), (int)(vImage.Height*ScaleConst));
			
			BitmapData vbmpDataOut = vImageOut.LockBits(new Rectangle(0,0,vImageOut.Width,vImageOut.Height),
                 System.Drawing.Imaging.ImageLockMode.ReadWrite, vImageOut.PixelFormat);
			byte[] vImageDataOut = new byte[vbmpDataOut.Stride * vbmpDataOut.Height];
			    System.Runtime.InteropServices.Marshal.Copy(vbmpDataOut.Scan0,vImageDataOut,0
			                                            ,vImageDataOut.Length);
			vImageOut.UnlockBits(vbmpDataOut);

			
			double dist = 99999;
			int minx =0, miny=0;			
			
			int steploop = 20;
			for(int iy = k; iy < vInImageArray.GetUpperBound(0) - k+1; iy = iy+steploop/*iy++*/)
				for(int ix = k; ix < vInImageArray.GetUpperBound(1)-k+1; ix = ix+steploop/*ix++*/)
					foreach(byte[,] tt in SampledImages)
					{					
						for(int yy = k; yy < tt.GetUpperBound(0) - k+1; yy = yy+steploop/*yy++*/)
							for(int xx = k; xx < tt.GetUpperBound(1) - k+1; xx = xx+steploop/*xx++*/)
						{
							double dd = Dist(iy,ix,yy,xx, vInImageArray, tt);
							if(dd < dist)
							{
								dist = dd;
								minx = xx;
								miny = yy;
							}													
						}
						int srcX = (int)((minx - kk)*SampleConst);
						int srcY = (int)((miny - kk)*SampleConst);
						int destX = (int)((ix - kk)*ScaleConst);
						int destY = (int)((iy - kk)*ScaleConst);
						ChangePic(vImage, vImageDest, srcX, srcY, destX, destY, SampleConst, ScaleConst);
					}
			
			}
				
		
		//перекодировка в yiq
		void CodeYIQ(Bitmap vInImage, byte[,] vInImageArray)
		{
						
			BitmapData vbmpDataI;
			vbmpDataI= vInImage.LockBits(new Rectangle(0,0,vInImage.Width,vInImage.Height),
                 System.Drawing.Imaging.ImageLockMode.ReadWrite, vInImage.PixelFormat);
			byte[] vImageData = new byte[vbmpDataI.Stride * vbmpDataI.Height];
			    System.Runtime.InteropServices.Marshal.Copy(vbmpDataI.Scan0,vImageData,0
			                                            ,vImageData.Length);
			vInImage.UnlockBits(vbmpDataI);
			
			int vStep = 4;
			double xConst=0.299, yConst=0.587, zConst=0.114;
			
			for (int i = 0; i < vInImage.Height; i++)
				for (int j = 0; j < vInImage.Width; j++)
					vInImageArray[i, j] = (byte)(xConst*vImageData[(i * vInImage.Width + j) * vStep + 2]
						+yConst*vImageData[(i * vInImage.Width + j) * vStep + 1]
						+zConst*vImageData[(i * vInImage.Width + j) * vStep]);
			
		}
		
		
		
		double Dist(int Ai, int Aj, int Bi, int Bj, byte[,] A, byte[,] B)
		{
			int sum = 0;
			for(int i = -k; i < k+1; i++)
				for(int j = -k; j < k+1; j++)
			{
				int l = A[Ai+i, Aj+j] - B[Bi+i, Bj+j];
				sum += l*l;
			}
			return sum;
		}
		
		void ChangePic(Bitmap srcImage, Bitmap destImage, int srcX, int srcY, int destX, int destY, double srcCoef, double destCoef)
		{
			
				
		  using (Graphics graphic = Graphics.FromImage(destImage))
        {
            graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphic.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphic.Clear(SystemColors.Control); 
            
            
    		// Create rectangle for dest image.
    		int destK = (int)(patchSize*destCoef);
    		Rectangle destRect = new Rectangle(destX, destY, destK, destK);

    		// Create rectangle for source image.
    		int srcK = (int)(patchSize*srcCoef);
    		Rectangle srcRect = new Rectangle(srcX, srcY, srcK, srcK);
    		GraphicsUnit units = GraphicsUnit.Pixel;

    		// Draw image to screen.
    		graphic.DrawImage(srcImage, destRect, srcRect, units);
        }
	
		
		}
		

		
		}
	
		

	}
