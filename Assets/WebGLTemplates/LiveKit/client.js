var socket = io() || {}
socket.isReady = false

window.addEventListener('load', function () {
  var execInUnity = function (method) {
    if (!socket.isReady) return

    var args = Array.prototype.slice.call(arguments, 1)

    f(window.unityInstance != null)
    {
      //fit formats the message to send to the Unity client game, take a look in NetworkManager.cs in Unity
      window.unityInstance.SendMessage('NetworkManager', method, args.join(':'))
    }
  } //END_exe_In_Unity

  
  socket.on('TOKEN_GENERATED', function (token) {
    var currentUserAtr = token

    if (window.unityInstance != null) {
      window.unityInstance.SendMessage(
        'NetworkManager',
        'OnTokenGenerated',
        currentUserAtr
      )
    }
  }) //END_SOCKET.ON

}) //END_window_addEventListener