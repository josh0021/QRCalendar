using System;
using ZXing.Mobile;

#if __UNIFIED__
using UIKit;
using CoreGraphics;
#else
using MonoTouch.UIKit;
using CGRect = System.Drawing.RectangleF;
#endif

namespace Sample.iOS
{
    public class ImageViewController : UIViewController
    {
        public ImageViewController () : base ()
        {
        }
		public UITextField usernameField;
        UIImageView imageBarcode;

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

			View.BackgroundColor = UIColor.Gray;

			nfloat h = 31.0f;
			nfloat w = View.Bounds.Width;

			usernameField = new UITextField
			{
				Placeholder = "Enter your username",
				BorderStyle = UITextBorderStyle.RoundedRect,
				Frame = new CGRect(10, 82, w - 20, h)
			};

			View.AddSubview(usernameField);
            NavigationItem.Title = "Generate Barcode";

            imageBarcode = new UIImageView (new CGRect (220, 80, View.Frame.Width - 60, View.Frame.Height - 220));

            View.AddSubview (imageBarcode);

            var barcodeWriter = new BarcodeWriter {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions {
                    Width = 300,
                    Height = 300,
                    Margin = 30
                }
            };

            var barcode = barcodeWriter.Write ("ZXing.Net.Mobile");

            imageBarcode.Image = barcode;
        }
    }
}

