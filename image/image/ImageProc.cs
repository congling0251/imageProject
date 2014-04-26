using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;
using System.Drawing;


namespace image
{
    public class ImageProc
    {
        String path;
        Image<Bgr, Byte> img_src;
        int height;
        int width;
        IntPtr hist;
        Image<Hsv, Byte> img_hist;

        public ImageProc(string url)
        {
            path = url;
            img_src = new Image<Bgr, byte>(url);
            height = img_src.Height;
            width = img_src.Width;
        }

        //Save image to the assigned path
        public void SaveImage(string savePath)
        {
            img_src.Save(savePath);
        }

        //Generate and save miniature(for image list display), 
        //len is the width or height of the miniature
        public void SaveMiniImage(string savePath, int len)
        {
            double scale = 0;

            if (height > width)
                scale = (double)len / (double)height;
            else
                scale = (double)len / (double)width;

            Image<Bgr, Byte> img_mini = img_src.Resize(scale, INTER.CV_INTER_LINEAR);
            img_mini.Save(savePath);
        }

        //Calculate image's HSV histogram using unmanaged OpenCV invoke
        /*Need to know if managed emgu version is better*/
        /*Add import values to control bins and levels if necessary*/
        public void GetHSVHistogram(int[] v)
        {
            int h_bins = 16;
            int s_bins = 8;
            int[] hist_size = new int[2] { h_bins, s_bins };

            int h_level = 180;
            int s_level = 250;
            float[] h_range = new float[2] { 1, h_level };  //最小值为1，去除黑背景
            float[] s_range = new float[2] { 1, s_level };

            IntPtr inPtr1 = new IntPtr(0);
            IntPtr inPtr2 = new IntPtr(0);
            GCHandle gch1 = GCHandle.Alloc(h_range, GCHandleType.Pinned);
            GCHandle gch2 = GCHandle.Alloc(s_range, GCHandleType.Pinned);
            try
            {
                inPtr1 = gch1.AddrOfPinnedObject();
                inPtr2 = gch2.AddrOfPinnedObject();
            }
            finally
            {
                gch1.Free();
                gch2.Free();
            }
            IntPtr[] range = new IntPtr[2] { inPtr1, inPtr2 };

            //IntPtr img_ptr = img_src.Resize(200,200,INTER.CV_INTER_LINEAR).Ptr;   
            IntPtr img_ptr = img_src.Ptr;
            Size s = CvInvoke.cvGetSize(img_ptr);
            IntPtr img_hsv = CvInvoke.cvCreateImage(s, IPL_DEPTH.IPL_DEPTH_8U, 3);
            IntPtr h_plane = CvInvoke.cvCreateImage(s, IPL_DEPTH.IPL_DEPTH_8U, 1);
            IntPtr s_plane = CvInvoke.cvCreateImage(s, IPL_DEPTH.IPL_DEPTH_8U, 1);
            IntPtr v_plane = CvInvoke.cvCreateImage(s, IPL_DEPTH.IPL_DEPTH_8U, 1);

            IntPtr[] planes = new IntPtr[2] { h_plane, s_plane };

            CvInvoke.cvCvtColor(img_ptr, img_hsv, COLOR_CONVERSION.CV_BGR2HSV);
            CvInvoke.cvSplit(img_hsv, h_plane, s_plane, v_plane, IntPtr.Zero);

            hist = CvInvoke.cvCreateHist(2, hist_size, HIST_TYPE.CV_HIST_ARRAY, range, 1);
            CvInvoke.cvCalcHist(planes, hist, false, IntPtr.Zero);
            CvInvoke.cvNormalizeHist(hist, 1024);   //unify the number of pixels

            //准备生成直方图图像
            int bin_width = 5;
            int hist_width = h_bins * s_bins * bin_width;
            int hist_height = 240;

            img_hist = new Image<Hsv, byte>(hist_width, hist_height);

            float max_value = 0;
            float min_value = 0;
            int[] mi = { 0 };
            CvInvoke.cvGetMinMaxHistValue(hist, ref min_value, ref max_value, mi, mi);

            for (int hn = 0; hn < h_bins; ++hn)
                for (int sn = 0; sn < s_bins; ++sn)
                {
                    int i = hn * s_bins + sn;
                    double value = CvInvoke.cvQueryHistValue_2D(hist, hn, sn);
                    v[i] = (int)value;
                    //获取当前bin的颜色和长度
                    Hsv color_hsv = new Hsv(hn * 180f / h_bins, sn * 255f / s_bins, 255f);
                    int intense = (int)((value / (double)max_value) * hist_height);
                    //绘制直方图bin
                    Rectangle bin = new Rectangle(i * bin_width, height - intense, bin_width, intense);
                    img_hist.Draw(bin, color_hsv, 0);
                }
        }

        public void SaveHistImg(string url)
        {
            img_hist.Save(url);
        }

        public static ulong CalEuclidDistance(int[] c, int[] tc, int num)
        {
            ulong dis = 0;
            for (int i = 0; i < num; i++)
                dis += (ulong)((tc[i] - c[i]) * (tc[i] - c[i]));
            return dis;
        }

        public static ulong CalEuclidDistance(double[] c, double[] tc, int num)
        {
            ulong dis = 0;
            for (int i = 0; i < num; i++)
                dis += (ulong)((tc[i] - c[i]) * (tc[i] - c[i]));
            return dis;
        }

        public void GetHuMoments(Image<Gray, Byte> img, double[] m_hu)
        {
            MCvMoments moments = new MCvMoments();
            MCvHuMoments hu = new MCvHuMoments();

            CvInvoke.cvMoments(img, ref moments, 0);
            CvInvoke.cvGetHuMoments(ref moments, ref hu);

            m_hu[0] = hu.hu1;
            m_hu[1] = hu.hu2;
            m_hu[2] = hu.hu3;
            m_hu[3] = hu.hu4;
            m_hu[4] = hu.hu5;
            m_hu[5] = hu.hu6;
            m_hu[6] = hu.hu7;
        }

        public void GetShapeFeature(ref double[] m)
        {
            Image<Gray, Byte> img_hu = img_src.Convert<Gray, Byte>();

            Gray threshold = new Gray(100);
            Gray threshold_link = new Gray(80);

            img_hu = img_hu.Canny(threshold, threshold_link);

            GetHuMoments(img_hu, m);

            for (int i = 0; i < 7; i++)
            {
                double am = Math.Abs(m[i]);
                int sm;

                if (m[i] > 0)
                    sm = 1;
                else if (m[i] < 0)
                    sm = -1;
                else
                    sm = 0;

                m[i] = sm * Math.Log10(am);
            }
        }

    }
}