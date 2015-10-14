using System;
using MonoTouch.Dialog;

#if __UNIFIED__
using Foundation;
using CoreGraphics;
using UIKit;
#else
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CGRect = System.Drawing.RectangleF;
#endif

using ZXing;
using ZXing.Mobile;


namespace Sample.iOS
{
	public class HomeViewController : DialogViewController
	{		
        public HomeViewController () : base (UITableViewStyle.Grouped, new RootElement ("ZXing.Net.Mobile"), false)
		{
		}

		MobileBarcodeScanner scanner;
		CustomOverlayView customOverlay;
	
		public override void ViewDidLoad ()
		{			
			//Create a new instance of our scanner
			scanner = new MobileBarcodeScanner(this.NavigationController);

            Root = new RootElement ("ZXing.Net.Mobile") {
                new Section {
                    
                    new StyledStringElement ("Scan with Default View", async () => {
                        //Tell our scanner to use the default overlay
                        scanner.UseCustomOverlay = false;
                        //We can customize the top and bottom text of the default overlay
                        scanner.TopText = "Hold camera up to barcode to scan";
                        scanner.BottomText = "Barcode will automatically scan";

                        //Start scanning
                        var result = await scanner.Scan ();

                        HandleScanResult(result);
                    }),

                    new StyledStringElement ("Scan Continuously", () => {
                        //Tell our scanner to use the default overlay
                        scanner.UseCustomOverlay = false;

                        //Tell our scanner to use our custom overlay
                        scanner.UseCustomOverlay = true;
                        scanner.CustomOverlay = customOverlay;


                        var opt = new MobileBarcodeScanningOptions ();
                        opt.DelayBetweenContinuousScans = 3000;

                        //Start scanning
                        scanner.ScanContinuously (opt, true, HandleScanResult);
                    }),

                    new StyledStringElement ("Scan with Custom View", async () => {
                        //Create an instance of our custom overlay
                        customOverlay = new CustomOverlayView();
                        //Wireup the buttons from our custom overlay
                        customOverlay.ButtonTorch.TouchUpInside += delegate {
                            scanner.ToggleTorch();      
                        };
                        customOverlay.ButtonCancel.TouchUpInside += delegate {
                            scanner.Cancel();
                        };

                        //Tell our scanner to use our custom overlay
                        scanner.UseCustomOverlay = true;
                        scanner.CustomOverlay = customOverlay;

                        var result = await scanner.Scan ();

                        HandleScanResult(result);
                    }),

                    new StyledStringElement ("Scan with AVCapture Engine", async () => {
                        //Tell our scanner to use the default overlay
                        scanner.UseCustomOverlay = false;
                        //We can customize the top and bottom text of the default overlay
                        scanner.TopText = "Hold camera up to barcode to scan";
                        scanner.BottomText = "Barcode will automatically scan";

                        //Start scanning
                        var result = await scanner.Scan (true);

                        HandleScanResult (result);  
                    }),

                    new StyledStringElement ("Generate Barcode", () => {
                        NavigationController.PushViewController (new ImageViewController (), true);
                    })
                }
            };
		}

		void HandleScanResult(ZXing.Result result)
		{
			string msg = "";

			if (result != null && !string.IsNullOrEmpty (result.Text)) {
				msg = "Found Barcode: " + result.Text;
				UIImage captureImage = result.CaptureImage as UIImage;

				if (captureImage != null) {
					InvokeOnMainThread (() => {
						CGRect theRect = new CGRect(new CGPoint(0,0), new CGSize(200,400));
						UIImageView theView = new UIImageView(theRect);
						theView.ContentMode = UIViewContentMode.ScaleToFill;
						this.View.AddSubview(theView);
						UIGraphics.BeginImageContext(captureImage.Size);

						captureImage.Draw(CGPoint.Empty);

						CGContext ctx = UIGraphics.GetCurrentContext();

						ctx.SetStrokeColor(UIColor.Red.CGColor);
						ctx.SetLineWidth(2);
						ctx.MoveTo(result.ResultPoints[0].X, result.ResultPoints[0].Y);
						ctx.AddLineToPoint(result.ResultPoints[1].X, result.ResultPoints[1].Y);
						ctx.AddLineToPoint(result.ResultPoints[2].X, result.ResultPoints[2].Y);
						ctx.AddLineToPoint(result.ResultPoints[3].X, result.ResultPoints[3].Y);
						ctx.AddLineToPoint(result.ResultPoints[0].X, result.ResultPoints[0].Y);
						ctx.StrokePath();

						UIImage retImage = UIGraphics.GetImageFromCurrentImageContext();

						UIGraphics.EndImageContext();

						theView.Image = retImage;

					});

				}
			}
			else
				msg = "Scanning Canceled!";

			this.InvokeOnMainThread(() => {
				var av = new UIAlertView("Barcode Result", msg, null, "OK", null);
				av.Show();
			});
		}
	}
}

