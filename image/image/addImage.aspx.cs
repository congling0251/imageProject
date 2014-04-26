using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace image
{
    public partial class addImage : System.Web.UI.Page
    {
        private string path = "";
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string name = name_box.Text;
            string category = category_box.Text;
            string period = period_box.Text;
            string museum = museum_box.Text;
            string intro = intro_box.Text;
            System.Diagnostics.Debug.Write(Session["path"]);
            const int num = 128;
            //计算图像中已标定的文物目标的特征值
          ImageProc roiImg = new ImageProc(Session["path"].ToString());
            int[] hist_hsv = new int[num];
            roiImg.GetHSVHistogram(hist_hsv);
            //string str_hsv = GetCharaStr(hist_hsv, num);    //color descriptor
            Image_Color newColor = new Image_Color();
            System.Diagnostics.Debug.Write(hist_hsv);
            double[] shape_hu = new double[7];
            roiImg.GetShapeFeature(ref shape_hu);     //Shape descriptor
            Image_Shape newShape = new Image_Shape();
            System.Diagnostics.Debug.Write(shape_hu);
            SqlServerDataBase sdb = new SqlServerDataBase();

            string stroresqlstring = "insert into Image_info(Url,Tag,Intro,category,Museum,Period) values ('" + Image1.ImageUrl + "','" + name + "','" + intro + "','" + category + "','" + museum + "','" + period + "')";
            bool stroeFlag = sdb.Insert(stroresqlstring, null);
            if (stroeFlag == true)
            {
                Response.Write("<script>alert('保存成功！');</script>");
            }
            else
            {
                Response.Write("<script>alert(\"" + sdb.ErrorMessage + "\");</script>");
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            string virpath = "~/Images/image/";
            string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + FileUpload1.FileName; 
            path = Server.MapPath(virpath) + filename;//服务器保存路径
            FileUpload1.PostedFile.SaveAs(path);//保存
            Image1.ImageUrl = virpath + filename;
            Session["path"] = path;
        }
    }
}