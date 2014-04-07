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
		
		void Button1Click(object sender, EventArgs e)
		{
			Kernel();
		}
		
		void Kernel()
		{
			Bitmap vImage = new Bitmap(@"pic.png", true);
			
			byte[,] vInImageArray = new byte[vImage.Height,vImage.Width];
			CodeYIQ(vImage, vInImageArray);
						
			List<byte[,]> SampledImages = new List<byte[,]>();
			Bitmap vImageI = vImage;
			double SampleConst = 1.2;
			for(int i = 0; i < 3; i++)	
			{
				vImageI = new Bitmap(vImageI, (int)(vImageI.Width/SampleConst), (int)(vImageI.Height/SampleConst));
				
				byte[,] vInImageArrayI = new byte[vImageI.Height,vImageI.Width];
				CodeYIQ(vImageI, vInImageArrayI);
				SampledImages.Add(vInImageArrayI);
			}
						
			double dist = 99999;
			int minx, miny;			
			
			for(int iy = 4; iy < vInImageArray.GetUpperBound(1) - 3; iy++)
				for(int ix = 4; ix < vInImageArray.GetUpperBound(0)-3; ix++)
					foreach(byte[,] tt in SampledImages)
					{					
						for(int yy = 4; yy < tt.GetUpperBound(0) - 3; yy++)
							for(int xx = 4; xx < tt.GetUpperBound(1) - 3; xx++)
						{
							double dd = Dist(iy,ix,yy,xx, vInImageArray, tt);
							if(dd < dist)
							{
								dist = dd;
								minx = xx;
								miny = yy;
							}
							
						}
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
			for(int i = -4; i < 5; i++)
				for(int j = -4; j < 5; j++)
			{
				int l = A[Ai+i, Aj+j] - B[Bi+i, Bj+j];
				sum += l*l;
			}
			return sum;
		}
		
		
		
		}
	
		

	}
