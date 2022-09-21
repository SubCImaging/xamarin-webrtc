using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebRtc.Android.Code;
using WebRTC.Shared;
using Xam.WebRtc.Android;

namespace WebRtc.Android.Controllers
{
    public class WebRTCController : WebApiController
    {
        private readonly WebRtcClient client;

        public WebRTCController(WebRtcClient client)
        {
            this.client = client;
        }

        [Route(HttpVerbs.Put, "/hello/{id}")]
        public void Hello(string id)
        {
            Console.WriteLine(id);
            // Id = message.Id;
        }

        [Route(HttpVerbs.Put, "/receiveanswer/{message}")]
        public void ReceiveAnswer(SignalingMessage message)
        {
            client.ReceiveAnswer(
                    new SessionDescription(
                        SessionDescription.SdpType.Answer,
                        message.Sdp),
                    (sdp, err) =>
                    {
                    });
        }

        [Route(HttpVerbs.Put, "/receivecandidate/{message}")]
        public void ReceiveCandidate(SignalingMessage message)
        {
            client.ReceiveCandidate(new IceCandidate(
                    message.Candidate.SdpMid,
                    message.Candidate.SdpMLineIndex,
                    message.Candidate.Sdp));
        }

        [Route(HttpVerbs.Put, "/receiveoffer/{message}")]
        public void ReceiveOffer(SignalingMessage message)
        {
            client.ReceiveOffer(
                    new SessionDescription(
                        SessionDescription.SdpType.Offer,
                        message.Sdp),
                    (sdp, err) =>
                    {
                        if (string.IsNullOrEmpty(err))
                        {
                            var signal = new SignalingMessage()
                            {
                                Type = sdp.Type.ToString(),
                                Sdp = sdp.Description,
                                // Id = Id,
                            };

                            // SendViaSocket(signal);
                        }
                    });
        }
    }
}