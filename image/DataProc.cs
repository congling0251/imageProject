using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;


namespace image
{
    public class DataProc
    {
        CBIR_ImageDataContext ImageData = new CBIR_ImageDataContext();
        public DataProc()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }

        //Generate descriptor string for database storage,
        //intNum is the number of values
        public string GetCharaStr(int[] value, int intNum)
        {
            string str = "";
            for (int j = 0; j < intNum; j++)
                str += Convert.ToString(value[j]) + ' ';
            return str;
        }

        //Get descriptor values from descriptor string in the database
        public int[] ConvertCharaStr(string str, int num)
        {
            int[] CharaValue = new int[num];
            int index = 0;
            string valueStr;
            int value = 0;

            int i = 0;
            while ((index = str.IndexOf(' ')) != -1)
            {
                valueStr = str.Substring(0, index);
                value = int.Parse(valueStr);
                CharaValue[i] = value;

                str = str.Substring(index + 1, str.Length - index - 1);
                i++;
            }
            return CharaValue;
        }

        //Insert one image record into database
        public void InsertImage(string tag, int caid, int pid, int mid, string intro, string id)
        {
            const int num = 128;

            Image_Info newImg = new Image_Info();
            newImg.ID = GetNewId();
            newImg.Tag = tag;
            newImg.CategoryID = caid;
            newImg.PeriodID = pid;
            newImg.MuseumID = mid;
            newImg.Intro = intro;
            newImg.Url = id;

            ImageProc myImage = new ImageProc(System.Web.HttpContext.Current.Server.MapPath("~/image/temp/") + id);
            myImage.SaveImage(System.Web.HttpContext.Current.Server.MapPath("~/image/repo/") + id);
            myImage.SaveMiniImage(System.Web.HttpContext.Current.Server.MapPath("~/image/repo/mini/") + id, 200);

            //计算图像中已标定的文物目标的特征值
            ImageProc roiImg = new ImageProc(System.Web.HttpContext.Current.Server.MapPath("~/image/roi/") + id);
            int[] hist_hsv = new int[num];
            roiImg.GetHSVHistogram(hist_hsv);
            string str_hsv = GetCharaStr(hist_hsv, num);    //color descriptor
            Image_Color newColor = new Image_Color();

            double[] shape_hu = new double[7];
            roiImg.GetShapeFeature(ref shape_hu);     //Shape descriptor
            Image_Shape newShape = new Image_Shape();


            int newId;
            if (ImageData.Image_Color.Count() == 0)
                newId = 0;
            else
                newId = ImageData.Image_Color.Max(cid => cid.ID) + 1;

            newColor.ID = newId;
            newImg.ColorID = newId;
            newColor.HSV = str_hsv;

            newShape.ID = newId;
            newImg.ShapeID = newId;
            newShape.s1 = shape_hu[0];
            newShape.s2 = shape_hu[1];
            newShape.s3 = shape_hu[2];
            newShape.s4 = shape_hu[3];
            newShape.s5 = shape_hu[4];
            newShape.s6 = shape_hu[5];
            newShape.s7 = shape_hu[6];

            ImageData.Image_Shape.InsertOnSubmit(newShape);
            ImageData.Image_Color.InsertOnSubmit(newColor);
            ImageData.Image_Info.InsertOnSubmit(newImg);
            ImageData.SubmitChanges();
        }

        public int GetNewId()
        {
            int newId;
            if (ImageData.Image_Info.Count() == 0)
                newId = 0;
            else
                newId = ImageData.Image_Info.Max(id => id.ID) + 1;
            return newId;
        }

        public void UpdateImageInfo(int id, string name, int cid, int pid, int mid, string intro)
        {
            Image_Info img = GetSingleImgInfo(id);
            img.Tag = name;
            img.Image_Category = ImageData.Image_Category.Single(c => c.ID == cid);
            img.Image_Period = ImageData.Image_Period.Single(p => p.ID == pid);
            img.Image_Museum = ImageData.Image_Museum.Single(m => m.ID == mid);
            img.Intro = intro;

            ImageData.SubmitChanges();
        }

        public void DeleteImage(int id)
        {
            Image_Info img = GetSingleImgInfo(id);
            Image_Color imgClr = GetSingleImgClr(Convert.ToInt32(img.ColorID));
            Image_Shape imgShp = GetSingleImgShp(Convert.ToInt32(img.ShapeID));
            ImageData.Image_Info.DeleteOnSubmit(img);
            ImageData.Image_Color.DeleteOnSubmit(imgClr);
            ImageData.Image_Shape.DeleteOnSubmit(imgShp);

            ImageData.SubmitChanges();
        }
        public Image_Info GetSingleImgInfo(int id)
        {
            Image_Info img = ImageData.Image_Info.Single(c => c.ID == id);
            return img;
        }

        public Image_Color GetSingleImgClr(int clrId)
        {
            Image_Color imgClr = ImageData.Image_Color.Single(c => c.ID == clrId);
            return imgClr;
        }

        public Image_Shape GetSingleImgShp(int shpId)
        {
            Image_Shape imgShp = ImageData.Image_Shape.Single(c => c.ID == shpId);
            return imgShp;
        }

        public IEnumerable<Image_Info> GetColorSearchResults()
        {
            IEnumerable<Image_Info> rs = from r in ImageData.Image_Info
                                         orderby r.Image_Color.Distant ascending
                                         select r;
            return rs;
        }

        public IEnumerable<Image_Info> GetShapeSearchResults()
        {
            IEnumerable<Image_Info> rs = from r in ImageData.Image_Info
                                         orderby r.Image_Shape.Distant ascending
                                         select r;
            return rs;
        }

        public IEnumerable<Image_Info> GetRandAllInfo()
        {
            IEnumerable<Image_Info> ran = from d in ImageData.Image_Info
                                          orderby ImageData.NEWID()
                                          select d;
            return ran;
        }

        public IEnumerable<Image_Info> GetAllInfo()
        {
            IEnumerable<Image_Info> lst = from c in ImageData.Image_Info
                                          orderby c.Url
                                          select c;
            return lst;
        }

        public IEnumerable<Image_Color> GetAllClr()
        {
            IEnumerable<Image_Color> clr = from c in ImageData.Image_Color
                                           select c;
            return clr;
        }

        public IEnumerable<Image_Shape> GetAllShp()
        {
            IEnumerable<Image_Shape> Shp = from s in ImageData.Image_Shape
                                           select s;
            return Shp;
        }

        public void MatchShape(string url)
        {
            double[] ts = new double[7];
            double[] s = new double[7];
            ulong disS = 0;

            ImageProc myImage = new ImageProc(url);
            myImage.GetShapeFeature(ref s);

            var shape = GetAllShp();

            foreach (Image_Shape sd in shape)
            {
                disS = 0;

                ts[0] = (double)sd.s1;
                ts[1] = (double)sd.s2;
                ts[2] = (double)sd.s3;
                ts[3] = (double)sd.s4;
                ts[4] = (double)sd.s5;
                ts[5] = (double)sd.s6;
                ts[6] = (double)sd.s7;

                disS = ImageProc.CalEuclidDistance(s, ts, 4);
                sd.Distant = Math.Sqrt(disS);
            }

            ImageData.SubmitChanges();
        }

        public void MatchColor(string dir, string id)
        {
            const int num = 128;    //the number of hsv histogram's value

            ulong dis_hsv = 0;

            int[] trg_hsv = new int[num];
            int[] src_hsv = new int[num];

            ImageProc myImage = new ImageProc(dir + id);
            myImage.GetHSVHistogram(src_hsv);

            myImage.SaveHistImg(HttpContext.Current.Server.MapPath("~/image/hist/") + id);

            var chara = GetAllClr();

            foreach (Image_Color cd in chara)
            {
                trg_hsv = ConvertCharaStr(cd.HSV, num);

                dis_hsv = ImageProc.CalEuclidDistance(src_hsv, trg_hsv, 128);

                cd.Distant = (double)(Math.Sqrt(dis_hsv));
            }
            ImageData.SubmitChanges();
        }

        public void MatchColorNew(string url)
        {

        }

        public void ReCalColor()
        {
            const int num = 128;

            var imgInfo = GetAllInfo();
            foreach (Image_Info img in imgInfo)
            {
                //使用原图片
                //ImageProc myImage = new ImageProc(System.Web.HttpContext.Current.Server.MapPath("~/image_repository/") + img.Url);
                //使用标记处理过的图片
                ImageProc myImage = new ImageProc(HttpContext.Current.Server.MapPath("~/image/roi/") + img.Url);
                int[] hist_hsv = new int[num];
                myImage.GetHSVHistogram(hist_hsv);
                string str_hsv = GetCharaStr(hist_hsv, num);

                Image_Color imgClr = GetSingleImgClr((int)img.ColorID);
                imgClr.HSV = str_hsv;
            }
            ImageData.SubmitChanges();
        }
        public void ReCalShape()
        {
            double[] m_hu = new double[7];

            var imgInfo = GetAllInfo();
            foreach (Image_Info img in imgInfo)
            {
                //ImageProc myImage = new ImageProc(System.Web.HttpContext.Current.Server.MapPath("~/image_repository/") + img.Url);
                //使用标记处理过的图片
                ImageProc myImage = new ImageProc(HttpContext.Current.Server.MapPath("~/image/roi/") + img.Url);
                myImage.GetShapeFeature(ref m_hu);

                Image_Shape imgShp = GetSingleImgShp((int)img.ShapeID);
                imgShp.s1 = m_hu[0];
                imgShp.s2 = m_hu[1];
                imgShp.s3 = m_hu[2];
                imgShp.s4 = m_hu[3];
                imgShp.s5 = m_hu[4];
                imgShp.s6 = m_hu[5];
                imgShp.s7 = m_hu[6];
            }
            ImageData.SubmitChanges();
        }

    }
}