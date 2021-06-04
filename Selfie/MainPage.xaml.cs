using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Selfie
{
    public sealed partial class MainPage : Page
    {
        private MediaCapture mediaCapture;
        private const int maxFrames = 10;
        private int frame = 0;
        private int movieFrame = 0;
        private double frameTime = 0.1;
        DisplayRequest displayRequest = new DisplayRequest();
        private DispatcherTimer timer = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("navigated to");
            base.OnNavigatedTo(e);

            await StartCaptureAsync();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {

            base.OnNavigatedFrom(e);
            await StopCaptureAsync();
        }
        private async Task StartCaptureAsync()
        {
            try
            {
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"Error initializing camera:{ex.Message}");
            }
            try
            {
                preview.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"error starting capture:{ex.Message}");
            }
        }
        private async Task StopCaptureAsync()
        {
            if(mediaCapture != null)
            {
                await mediaCapture?.StartPreviewAsync();
            }
        }
        private async void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            LowLagPhotoCapture lowLagPhotoCapture = await mediaCapture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));
            CapturedPhoto captured = await lowLagPhotoCapture.CaptureAsync();
            SoftwareBitmap software = captured.Frame.SoftwareBitmap;
            await lowLagPhotoCapture.FinishAsync();

            if(software.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                software.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                software = SoftwareBitmap.Convert(software, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            SoftwareBitmapSource source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(software);
            ((Image)(pics.Children[frame++ % maxFrames])).Source = source;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Interval = TimeSpan.FromSeconds(slider.Value);
            timer.Tick += Timer_tick;
            for(int i = 0; i< maxFrames; i++)
            {
                Image image = new Image();
                image.Width = 100;
                image.Height = 100;
                image.PointerEntered += Image_PointerEntered;
                image.PointerExited += Image_PointerExited;
                pics.Children.Add(image);

            }
        }

        private void Timer_tick(object sender, object e)
        {
            int numFrames = (frame >= maxFrames) ? maxFrames : frame;
            Image image = ((Image)(pics.Children[movieFrame++ % numFrames]));
            playback.Source = image.Source;
        }

        private void Image_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            playback.Source = null;
        }

        private void Image_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Image image = sender as Image;
            if(image != null && image.Source != null)
            {
                playback.Source = image.Source;

            }

        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            movieFrame = 0;
            timer.Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            playback.Source = null;
        }

        private void slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            timer.Interval = TimeSpan.FromSeconds(slider.Value);
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            SymbolIcon icon = this.btnToggle.FindName("icon") as SymbolIcon;
            //if(icon.Symbol == SymbolIcon.SymbolProperty.Equals("Play")
        }
    }
}
