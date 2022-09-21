var localVideo;
var remoteVideo;
var localStream;
var myName;
var uuid;
var name; 
var connectedUser;
var peerConn;


var peerConnectionConfig = {
   'iceServers': [
     {'urls': 'stun:stun.stunprotocol.org:3478'},
     {
      urls: "turns:useastturn.subcstream.com:443",
      username: "username",
      credential: "password",
    },
   ]
};

serverConnection = new WebSocket('wss://' + window.location.hostname + ':8080');

serverConnection.onopen = function () { 
  console.log("Connected to the signaling server"); 
};

serverConnection.onmessage = gotMessageFromServer;

// document.getElementById('otherElements').hidden = true;
// var usernameInput = document.querySelector('#usernameInput'); 
var usernameShow = document.querySelector('#showLocalUserName'); 
var showAllUsers = document.querySelector('#allUsers');
var callBtn = document.querySelector('#callBtn'); 
var hangUpBtn = document.querySelector('#hangUpBtn');
// var loginBtn = document.querySelector('#loginBtn');

// Login when the user clicks the button 
// loginBtn.addEventListener("click", function (event) { 
//   name = usernameInput.value; 
//   usernameShow.innerHTML = "Hello, "+name;
//   if (name.length > 0) { 
//      send({ 
//         type: "login", 
//         name: name 
//      }); 
//   } 
 
// });


/* START: Register user for first time i.e. Prepare ground for webrtc call to happen */
// function handleLogin(success,allUsers) { 
//   if (success === false) { 
//     alert("Ooops...try a different username"); 
//   } 
//   else { 
    
    // var allAvailableUsers = allUsers.join();
    // console.log('All available users',allAvailableUsers)
    // showAllUsers.innerHTML = 'Available users: '+allAvailableUsers;
    localVideo = document.getElementById('localVideo');
    remoteVideo = document.getElementById('remoteVideo');
    // document.getElementById('myName').hidden = true;
    // document.getElementById('otherElements').hidden = false;

  

  var constraints = {
    video: true,
    audio: false
  };

  /* START:The camera stream acquisition */
  if(navigator.mediaDevices.getUserMedia) {
    navigator.mediaDevices.getUserMedia(constraints).then(getUserMediaSuccess);//.catch(errorHandler);
  } else {
    alert('Your browser does not support getUserMedia API');
  }
  /* END:The camera stream acquisition */
//   }
// }
/* END: Register user for first time i.e. Prepare ground for webrtc call to happen */


function getUserMediaSuccess(stream) {
  localStream = stream;
  localVideo.srcObject = stream;
  peerConn = new RTCPeerConnection(peerConnectionConfig);
  peerConn.addStream(localStream);
  peerConn.ontrack = gotRemoteStream;


  // peerConn.onicecandidate = function (event) { 
  //   console.log('onicecandidate call other user', event.candidate)
  //   if (event.candidate) { 
  //      send({ 
  //         type: "candidate", 
  //         candidate: event.candidate 
  //      }); 
  //   } 
  // }; 
}

callBtn.addEventListener("click", function () {
  console.log('inside call button')

  // var callToUsername = document.getElementById('callToUsernameInput').value;
	
  // if (callToUsername.length > 0) { 
  //   connectedUser = callToUsername; 
  //   console.log('nameToCall',connectedUser);
  //   console.log('create an offer to-',connectedUser)

    
    var connectionState2 = peerConn.connectionState;
    console.log('connection state before call beginning',connectionState2)
    var signallingState2 = peerConn.signalingState;
  //console.log('connection state after',connectionState1)
  console.log('signalling state after',signallingState2)
    peerConn.createOffer(function (offer) { 
      peerConn.setLocalDescription(offer);

        console.log(offer);

       send(offer); 
    
        
    }, function (error) { 
       alert("Error when creating an offer",error); 
       console.log("Error when creating an offer",error)
    }); 
    document.getElementById('callOngoing').style.display = 'block';
    document.getElementById('callInitiator').style.display = 'none';

   

  // } 
  // else 
  //   alert("username can't be blank!")


})


/* START: Recieved call from server i.e. recieve messages from server  */
function gotMessageFromServer(message) {
  console.log("Got message", message.data); 
  var data = JSON.parse(message.data); 
 
  switch(data.type) { 
    case "login": 
      handleLogin(data.success,data.allUsers); 
    break; 
    case "offer": 
      console.log('inside offer')
      handleOffer(data.offer, data.name); 
    break; 
    case "answer": 
      console.log('inside answer')
      handleAnswer(data.answer); 
    break; 
    case "candidate": 
      console.log('inside handle candidate')
      handleCandidate(data.candidate); 
    break; 
     //when somebody wants to call us 
    case "leave": 
      handleLeave(); 
    break; 
    default: 
      break; 
  } 

  serverConnection.onerror = function (err) { 
    console.log("Got error", err); 
  };

}

function send(msg) { 
  //attach the other peer username to our messages 
  if (connectedUser) { 
    msg.name = connectedUser; 
  } 
  console.log('msg before sending to server',msg)
  serverConnection.send(JSON.stringify(msg)); 
};

/* START: Create an answer for an offer i.e. send message to server */
function handleOffer(offer, name) { 
  // document.getElementById('callInitiator').style.display = 'none';
  // document.getElementById('callReceiver').style.display = 'block';

  /* Call answer functionality starts */
  // answerBtn.addEventListener("click", function () { 

  connectedUser = name; 
  peerConn.setRemoteDescription(new RTCSessionDescription(offer)); 
 
  //create an answer to an offer 
  peerConn.createAnswer(function (answer) { 
    console.log('going to create an answer for->', connectedUser)
    peerConn.setLocalDescription(answer); 
   
    send({ 
      type: "answer", 
        answer: answer 
    });
   
  }, function (error) { 
     alert("Error when creating an answer"); 
  }); 
  document.getElementById('callInitiator').style.display = 'none';
  document.getElementById('callOngoing').style.display = 'block';
// });
/* Call answer functionality ends */
/* Call decline functionality starts */
// declineBtn.addEventListener("click", function () {
//   document.getElementById('callInitiator').style.display = 'block';
//   document.getElementById('callReceiver').style.display = 'none';

// });

/*Call decline functionality ends */
};

//when we got an answer from a remote user 
function handleAnswer(answer) { 
  console.log('answer: ', answer)
  peerConn.setRemoteDescription(new RTCSessionDescription(answer)); 
};

//when we got an ice candidate from a remote user 
function handleCandidate(candidate) { 
  peerConn.addIceCandidate(new RTCIceCandidate(candidate)); 
};

function gotRemoteStream(event) {
  console.log('got remote stream');
  remoteVideo.srcObject = event.streams[0];
}

//hang up
hangUpBtn.addEventListener("click", function () { 
  send({ 
     type: "leave" 
  }); 
 
  handleLeave(); 
  document.getElementById('callOngoing').style.display = 'none';
  document.getElementById('callInitiator').style.display = 'block';
  
});

function handleLeave() { 
  if (peerConn) {
    peerConn.ontrack = null;
    
    peerConn.onicecandidate = null;
    peerConn.oniceconnectionstatechange = null;
    peerConn.onsignalingstatechange = null;
    peerConn.onicegatheringstatechange = null;
    peerConn.onnegotiationneeded = null;

    if (remoteVideo.srcObject) {
      remoteVideo.srcObject.getTracks().forEach(track => track.stop());
    }

    if (localVideo.srcObject) {
      localVideo.srcObject.getTracks().forEach(track => track.stop());
    }

    peerConn.close();
    peerConn = null;
  }

  remoteVideo.removeAttribute("src");
  remoteVideo.removeAttribute("srcObject");
  localVideo.removeAttribute("src");
  remoteVideo.removeAttribute("srcObject");
  document.getElementById('callOngoing').style.display = 'none';
  document.getElementById('callInitiator').style.display = 'block';
};


