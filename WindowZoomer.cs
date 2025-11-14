using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace 控件缩放算法
{
    /// <summary>
    /// 窗口控件缩放器
    /// </summary>
    public class WindowZoomer
    {
        //位置类型
        private class Location
        {
            public int X, Y;
            public Location(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        //窗口实例
        private Form RootForm { get; }
        private bool FontZoomer = true;
        //窗口原大小寄存
        private Size WindowSizeTemp = new Size();
        private Font WindowFontTemp;
        private List<Size> ControlSizeTemp = new List<Size>();
        private List<Location> ControlLocationTemp = new List<Location>();
        //X、Y轴的缩放系数
        private double ZoomeNum_X = 1.000;
        private double ZoomeNum_Y = 1.000;
        /// <summary>
        /// 初始化方法
        /// </summary>
        /// <param name="form">指定要建立缩放的窗口</param>
        public WindowZoomer(Form form, bool fontzoomer = true)
        {
            RootForm = form;
            FontZoomer = fontzoomer;
            WindowSizeTemp = RootForm.ClientSize;
            WindowFontTemp = RootForm.Font;
            void traverse(Control cont)
            {
                foreach (Control s in cont.Controls)
                {
                    ControlSizeTemp.Add(s.Size);//存储控件初始大小
                    ControlLocationTemp.Add(new Location(s.Left, s.Top));//存储控件初始位置
                    //如果该控件包含其它控件
                    if (s.Controls != null && s.Controls.Count > 0)
                        traverse(s);
                }
            }
            traverse(RootForm);
            RootForm.SizeChanged += RootForm_SizeChanged;
        }
        /// <summary>
        /// 窗口大小变更处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RootForm_SizeChanged(object sender, EventArgs e)
        {
            //计算缩放系数
            ZoomeNum_X = (double)((double)RootForm.ClientSize.Width / (double)WindowSizeTemp.Width);
            ZoomeNum_Y = (double)((double)RootForm.ClientSize.Height / (double)WindowSizeTemp.Height);
            if (FontZoomer)
            {
                //窗口全局字体缩放
                //检查窗口属性设置是否正确
                if (RootForm.AutoScaleMode != AutoScaleMode.None)
                    RootForm.AutoScaleMode = AutoScaleMode.None;
                float scalenum = (float)((RootForm.ClientSize.Width * RootForm.ClientSize.Height) / (float)(WindowSizeTemp.Width * WindowSizeTemp.Height));
                float q = (float)((double)WindowFontTemp.Size * scalenum);
                if (q <= 0) q = 0.01f;
                RootForm.Font = new Font(WindowFontTemp.Name, q);
            }
            //遍历控件列表应用新的控件属性
            int i = 0;
            void traverse(Control cont)
            {
                foreach (Control s in cont.Controls)
                {
                    //应用新的控件大小
                    s.Width = (int)((double)ControlSizeTemp[i].Width * ZoomeNum_X);
                    s.Height = (int)((double)ControlSizeTemp[i].Height * ZoomeNum_Y);
                    //应用新的控件位置
                    s.Left = (int)((double)ControlLocationTemp[i].X * ZoomeNum_X);
                    s.Top = (int)((double)ControlLocationTemp[i].Y * ZoomeNum_Y);
                    i++;
                    //如果该控件包含其它控件
                    if (s.Controls != null && s.Controls.Count > 0)
                        traverse(s);
                }
            }
            i = 0;
            traverse(RootForm);
        }
    }
}
