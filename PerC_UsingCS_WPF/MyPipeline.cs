using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PerC_UsingCS_WPF
{
    class MyPipeline : UtilMPipeline
    {
        public MyPipeline() : base()
        {
            EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_RGB32);
            EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH);
            EnableGesture();
        }

        public override void OnImage(PXCMImage image)
        {
            #region 攝影機影像顯示
            //Debug.WriteLine("收到影像,格式 : " + image.imageInfo.format);
            PXCMSession session = QuerySession();
            Bitmap bitmapimage; 
            image.QueryBitmap(session, out bitmapimage);
            BitmapSource source = ToWpfBitmap(bitmapimage);
            if (image.imageInfo.format == PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH)
                MainWindow.mainwin.ProcessDepthImage(source);
            else if (image.imageInfo.format == PXCMImage.ColorFormat.COLOR_FORMAT_RGB24)
                MainWindow.mainwin.ProcessColorImage(source);
            image.Dispose();
            #endregion

            PXCMGesture gesture = QueryGesture();
            if (gesture != null)
            {
                PXCMGesture.GeoNode node;
                //PXCMGesture.GeoNode[] nodes = new PXCMGesture.GeoNode[11];
                gesture.QueryNodeData(0, NeedNode() , out node);
                //gesture.QueryNodeData(0, NeedNodes(), nodes);
                ProcessNode(node);
                //ProcessNodes(nodes);
            }
        }

        public void  ProcessNode(PXCMGesture.GeoNode node)
        {
            Debug.WriteLine("Label=" + node.body);
            PXCMPoint3DF32 pos = node.positionImage;
            if ((node.body & PXCMGesture.GeoNode.Label.LABEL_FINGER_RING) != 0)
            {
                Debug.WriteLine("無名指 X=" + pos.x + ", Y=" + pos.y);
                MainWindow.mainwin.MoveEllipse2((int)pos.x, (int)pos.y);
            }
            else if ((node.body & PXCMGesture.GeoNode.Label.LABEL_FINGER_INDEX) != 0)
            {
                Debug.WriteLine("食指 X=" + pos.x + ", Y=" + pos.y);
                MainWindow.mainwin.MoveEllipse1((int)pos.x, (int)pos.y);
            }
        }
        public void ProcessNodes(PXCMGesture.GeoNode[] nodes)
        {
            Debug.WriteLine("Nodes :");
            foreach (var node in nodes)
            {
                //處理所有收到的節點
            }
        }
        public PXCMGesture.GeoNode.Label NeedNode()
        {
            return PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY |
                    PXCMGesture.GeoNode.Label.LABEL_FINGER_INDEX;
        }
        public PXCMGesture.GeoNode.Label NeedNodes()
        {
            return PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY;
        }


        public static BitmapSource ToWpfBitmap(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
    }
}
