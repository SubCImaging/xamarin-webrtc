using System;

namespace WebRTC.Shared
{
    public class Candidate
    {
        public string Sdp { get; set; }
        public string SdpMid { get; set; }
        public int SdpMLineIndex { get; set; }
    }

    public class SignalingMessage
    {
        public Candidate Candidate { get; set; }
        public string Id { get; set; }
        public string Sdp { get; set; }
        public string Type { get; set; }
    }
}