using Android.App;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using EmbedIO;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using Square.OkHttp3;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WebRtc.Android.Code;
using WebRtc.Android.Controllers;
using WebRtc.Android.Observers;
using WebRTC.Shared;
using Xam.WebRtc.Android;
using Xamarin.Essentials;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using Orientation = Android.Widget.Orientation;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WebRtc.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IWebRtcObserver
    {
        private SurfaceViewRenderer localView;

        //private SurfaceViewRenderer remoteView;
        //private IWebSocket socket;

        private WebRtcClient webRtcClient;

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            System.Diagnostics.Debug.WriteLine("Information: here");
            base.OnCreate(savedInstanceState);

            // Server must be started, before WebView is initialized,
            // because we have no reload implemented in this sample.
            Task.Factory.StartNew(async () =>
            {
                using var server = new WebServer(HttpListenerMode.EmbedIO, "http://*:8080");

                server.WithLocalSessionManager()
                        .WithCors()
                        .WithWebApi("/api/webrtc", m => m.WithController(() => new WebRTCController(webRtcClient)));

                await server.RunAsync();
            });

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            var connectButton = FindViewById<Button>(Resource.Id.connect_button);
            connectButton.Click += ConnectButton;
            var disconnectButton = FindViewById<Button>(Resource.Id.disconnect_button);
            disconnectButton.Click += DisconnectButton;

            var waveButton = FindViewById<Button>(Resource.Id.wave_button);
            waveButton.Text = "👋";
            waveButton.Click += (sender, args) => webRtcClient.SendMessage(waveButton.Text);

            //remoteView = FindViewById<SurfaceViewRenderer>(Resource.Id.remote_video_view);
            localView = FindViewById<SurfaceViewRenderer>(Resource.Id.local_video_view);

            // Force audio output to loudspeaker
            //var audioManager = (AudioManager)GetSystemService(AudioService);
            //audioManager.Mode = Mode.InCall;
            //audioManager.SpeakerphoneOn = true;

            RunOnUiThread(async () => await Init());
        }

        private void ConnectButton(object sender, EventArgs e)
        {
            webRtcClient.Connect(async (sdp, err) =>
            {
                if (string.IsNullOrEmpty(err))
                {
                    var message = new SignalingMessage()
                    {
                        Type = sdp.Type.ToString(),
                        Sdp = sdp.Description,
                        Id = Id,
                    };

                    using var h = new HttpClient { BaseAddress = new Uri(@"http://192.168.2.20:7529/webrtc/") };

                    var r = await h.PostAsJsonAsync("api/connect", message);

                    // var response = await h.PostAsJsonAsync("api/activation", request).ConfigureAwait(false);

                    // SendViaSocket(signal);
                }
            });
        }

        private void DisconnectButton(object sender, EventArgs e)
        {
            webRtcClient.Disconnect();
        }

        private async Task Init()
        {
            var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            var micStatus = await Permissions.RequestAsync<Permissions.Microphone>();

            webRtcClient = new WebRtcClient(this, localView, this);

            var dialogTcs = new TaskCompletionSource<string>();

            var linearLayout = new LinearLayout(this);
            linearLayout.Orientation = Orientation.Vertical;
            linearLayout.SetPadding(48, 24, 48, 24);
            var ipAddr = new EditText(this) { Hint = "IP Address", Text = "192.168.1.119" };
            var port = new EditText(this) { Hint = "Port", Text = "8080" };
            linearLayout.AddView(ipAddr);
            linearLayout.AddView(port);

            var alert = new AlertDialog.Builder(this)
                .SetTitle("Socket Address")
                .SetView(linearLayout)
                .SetPositiveButton("OK", (sender, args) =>
                {
                    dialogTcs.TrySetResult($"ws://{ipAddr.Text}:{port.Text}");
                })
                .Create();

            alert.Show();

            System.Diagnostics.Debug.WriteLine("Information: here");

            var wsUrl = await dialogTcs.Task;

            using var h = new HttpClient { BaseAddress = new Uri(@"http://192.168.2.20:5001/api/webrtc/") };

            var r = await h.PostAsJsonAsync("api/connect", new SignalingMessage { Type = "Hello" });

            System.Diagnostics.Debug.WriteLine("Information: " + r.StatusCode);

            //var okHttpClient = new OkHttpClient.Builder()
            //    .ReadTimeout(0, Java.Util.Concurrent.TimeUnit.Milliseconds)
            //    .Build();

            //var request = new Request.Builder()
            //    .Url(wsUrl)
            //    .Build();

            //socket = okHttpClient.NewWebSocket(
            //    request,
            //    new WebSocketObserver(ReadMessage));
        }

        //private void ReadMessage(SignalingMessage message)
        //{
        //    Console.WriteLine("Information: " + message.ToString());

        //    //if (message.Type == "hello")
        //    //{
        //    //    Id = message.Id;
        //    //}
        //    //else if (message.Type?.Equals(SessionDescription.SdpType.Offer.ToString(), StringComparison.OrdinalIgnoreCase) == true)
        //    //{
        //    //    Console.WriteLine("Information: received offer");

        //    //    webRtcClient.ReceiveOffer(
        //    //        new SessionDescription(
        //    //            SessionDescription.SdpType.Offer,
        //    //            message.Sdp),

        //    //        (sdp, err) =>
        //    //        {
        //    //            if (string.IsNullOrEmpty(err))
        //    //            {
        //    //                var signal = new SignalingMessage()
        //    //                {
        //    //                    Type = sdp.Type.ToString(),
        //    //                    Sdp = sdp.Description,
        //    //                    Id = Id,
        //    //                };
        //    //                SendViaSocket(signal);
        //    //            }
        //    //        });
        //    //}
        //    //else if (message.Type?.Equals(SessionDescription.SdpType.Answer.ToString(), StringComparison.OrdinalIgnoreCase) == true)
        //    //{
        //    //    Console.WriteLine("Information: received answer");
        //    //    webRtcClient.ReceiveAnswer(
        //    //        new SessionDescription(
        //    //            SessionDescription.SdpType.Answer,
        //    //            message.Sdp),
        //    //        (sdp, err) =>
        //    //        {
        //    //        });
        //    //}
        //    //else if (message.Candidate != null)
        //    //{
        //    //    Console.WriteLine("Information: candidate not null");

        //    //    webRtcClient.ReceiveCandidate(new IceCandidate(
        //    //        message.Candidate.SdpMid,
        //    //        message.Candidate.SdpMLineIndex,
        //    //        message.Candidate.Sdp));
        //    //}
        //}

        //private void SendViaSocket(SignalingMessage msg)
        //{
        //    var json = JsonConvert.SerializeObject(msg);
        //    //socket.Send(json);
        //}

        #region IWebRtcObserver

        public string Id { get; set; }

        public void OnConnectWebRtc()
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnConnectWebRtc)}");
        }

        public void OnDisconnectWebRtc()
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnDisconnectWebRtc)}");
        }

        public void OnGenerateCandiate(IceCandidate iceCandidate)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnGenerateCandiate)}");

            var can = new Candidate()
            {
                Sdp = iceCandidate.Sdp,
                SdpMLineIndex = iceCandidate.SdpMLineIndex,
                SdpMid = iceCandidate.SdpMid
            };

            var signal = new SignalingMessage()
            {
                Candidate = can,
                Id = Id,
            };

            //SendViaSocket(signal);
        }

        public void OnIceConnectionStateChanged(PeerConnection.IceConnectionState iceConnectionState)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnIceConnectionStateChanged)}");
        }

        public void OnOpenDataChannel()
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnOpenDataChannel)}");
        }

        public void OnReceiveData(byte[] data)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnReceiveData)}");
        }

        public void OnReceiveMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnReceiveMessage)}");

            var alert = new AlertDialog.Builder(this)
                .SetTitle("Message")
                .SetMessage(message)
                .SetPositiveButton("OK", (sender, args) => { })
                .Create();
            alert.Show();
        }

        #endregion IWebRtcObserver
    }
}