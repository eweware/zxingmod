using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;
using ZXing;
using ZXing.Mobile;
using Android.Graphics;

namespace Sample.Android
{
	[Activity (Label = "ZXing.Net.Mobile", MainLauncher = true, Theme="@android:style/Theme.Holo.Light", ConfigurationChanges=ConfigChanges.Orientation|ConfigChanges.KeyboardHidden)]
	public class Activity1 : Activity
	{
		Button buttonScanCustomView;
		Button buttonScanDefaultView;
        Button buttonContinuousScan;
		Button buttonFragmentScanner;
        Button buttonGenerate;
		Button hideImage;
		ImageView previewImage;


		MobileBarcodeScanner scanner;
	
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

            // Initialize the scanner first so we can track the current context
            MobileBarcodeScanner.Initialize (Application);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			//Create a new instance of our Scanner
			scanner = new MobileBarcodeScanner();
			hideImage = this.FindViewById<Button>(Resource.Id.hideImageBtn);
			previewImage = this.FindViewById<ImageView>(Resource.Id.previewImage);
			hideImage.Visibility = ViewStates.Gone;
			previewImage.Visibility = ViewStates.Gone;

			hideImage.Click += (object sender, System.EventArgs e) => {
				hideImage.Visibility = ViewStates.Gone;
				previewImage.Visibility = ViewStates.Gone;

			};

			buttonScanDefaultView = this.FindViewById<Button>(Resource.Id.buttonScanDefaultView);
			buttonScanDefaultView.Click += async delegate {
				
				//Tell our scanner to use the default overlay
				scanner.UseCustomOverlay = false;

				//We can customize the top and bottom text of the default overlay
				scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
				scanner.BottomText = "Wait for the barcode to automatically scan!";

				//Start scanning
				var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
				options.PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.AZTEC };
				var result = await scanner.Scan(options);

				HandleScanResult(result);
			};

            buttonContinuousScan = FindViewById<Button> (Resource.Id.buttonScanContinuous);
            buttonContinuousScan.Click += delegate {

                scanner.UseCustomOverlay = false;

                //We can customize the top and bottom text of the default overlay
                scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
                scanner.BottomText = "Wait for the barcode to automatically scan!";

                var opt = new MobileBarcodeScanningOptions ();
                opt.DelayBetweenContinuousScans = 3000;

                //Start scanning
                scanner.ScanContinuously(opt, HandleScanResult);
            };

			Button flashButton;
			View zxingOverlay;

			buttonScanCustomView = this.FindViewById<Button>(Resource.Id.buttonScanCustomView);
			buttonScanCustomView.Click += async delegate {

				//Tell our scanner we want to use a custom overlay instead of the default
				scanner.UseCustomOverlay = true;

				//Inflate our custom overlay from a resource layout
				zxingOverlay = LayoutInflater.FromContext(this).Inflate(Resource.Layout.ZxingOverlay, null);

				//Find the button from our resource layout and wire up the click event
				flashButton = zxingOverlay.FindViewById<Button>(Resource.Id.buttonZxingFlash);
				flashButton.Click += (sender, e) => scanner.ToggleTorch();

				//Set our custom overlay
				scanner.CustomOverlay = zxingOverlay;

				//Start scanning!
				var result = await scanner.Scan();

				HandleScanResult(result);
			};

			buttonFragmentScanner = FindViewById<Button> (Resource.Id.buttonFragment);
			buttonFragmentScanner.Click += delegate {
				StartActivity (typeof (FragmentActivity));	
			};

            buttonGenerate = FindViewById<Button> (Resource.Id.buttonGenerate);
            buttonGenerate.Click += delegate {
                StartActivity (typeof (ImageActivity));
            };
		}

		void HandleScanResult (ZXing.Result result)
		{
			string msg = "";

			if (result != null && !string.IsNullOrEmpty(result.Text))
				msg = "Found Barcode: " + result.Text;
			else
				msg = "Scanning Canceled!";

			this.RunOnUiThread(() => {
				Toast.MakeText(this, msg, ToastLength.Short).Show();
				hideImage.Visibility = ViewStates.Visible;
				previewImage.Visibility = ViewStates.Visible;
				Bitmap resultBitmap = result.CaptureImage as Bitmap;
				Bitmap canvasMap = resultBitmap.Copy(resultBitmap.GetConfig(), true);
				Canvas newCanvas = new Canvas(canvasMap);
				Paint thePaint = new Paint(PaintFlags.AntiAlias);

				thePaint.Color = Color.Red;
				thePaint.StrokeWidth = 2;
				newCanvas.DrawLine(result.ResultPoints[0].X, result.ResultPoints[0].Y, result.ResultPoints[1].X, result.ResultPoints[1].Y, thePaint);
				thePaint.Color = Color.Green;
				newCanvas.DrawLine(result.ResultPoints[1].X, result.ResultPoints[1].Y, result.ResultPoints[2].X, result.ResultPoints[2].Y, thePaint);
				thePaint.Color = Color.Blue;
				newCanvas.DrawLine(result.ResultPoints[2].X, result.ResultPoints[2].Y, result.ResultPoints[3].X, result.ResultPoints[3].Y, thePaint);
				thePaint.Color = Color.Yellow;
				newCanvas.DrawLine(result.ResultPoints[3].X, result.ResultPoints[3].Y, result.ResultPoints[0].X, result.ResultPoints[0].Y, thePaint);
				previewImage.SetImageBitmap(canvasMap);
			});
		}
	}
}


