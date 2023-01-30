// web3.jslib

mergeInto(LibraryManager.library, {
  WalletAddress: function WalletAddress() {
    var returnStr;
    try {
      // get address from metamask
      returnStr = web3.currentProvider.selectedAddress;
    } catch (e) {
      returnStr = "";
    }
    var returnStr = web3.currentProvider.selectedAddress;
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  MetamaskSignIn: function MetamaskSignIn() {
    window.ethereum.request({
      method: 'eth_requestAccounts'
    }).then(function (accounts) {
      account = accounts[0];
      console.log(account);

      //get the balance of the user and convert to MATIC
      window.ethereum.request({
        method: 'eth_getBalance',
        params: [account, 'latest']
      }).then(function (result) {
        console.log(result);
        var wei = parseInt(result, 16);
        var balance = wei / Math.pow(10, 18);
        console.log(balance + "MATIC");
        var currentUserAtr = account + ':' + balance;
        if (window.unityInstance != null) {
          //calls the OnMetamaskSignIn function of CanvasManager and sends the result to the unity application 
          window.unityInstance.SendMessage('CanvasManager', 'OnMetamaskSignIn', currentUserAtr);
        } //END_IF
      });
    });
  },

  Transaction: function Transaction() {
    var transactionParam = {
      to: '0xb22186477a77C9EFFF37FD2A199fC293e6d47cE6',
      from: account,
      value: '0x38D7EA4C68000'
    };

    //to prevent the transaction being used on the wrong chain

    window.ethereum.request({
      method: 'eth_sendTransaction',
      params: [transactionParam]
    }).then(function (txhash) {
      console.log(txhash);
      checkTransactionconfirmation(txhash).then(function (r) {
        return alert(r);
      });
    }); //END_REQUEST
  },

  MetamaskTransferTo: function MetamaskTransferTo(_myPublicAdrr, _to_public_address, _amount) {
    var currentUserAtr = '';
    var wei = parseFloat(UTF8ToString(_amount)) * Math.pow(10, 18);
    var transactionParam = {
      to: UTF8ToString(_to_public_address),
      from: UTF8ToString(_myPublicAdrr),
      value: wei.toString(16).toUpperCase() //convert to hex
    };

    window.ethereum.request({
      method: 'eth_sendTransaction',
      params: [transactionParam]
    }).then(function (txhash) {
      console.log(txhash);
      checkTransactionconfirmation(txhash).then(function (r) {
        alert(r);
        currentUserAtr = txhash;
        window.ethereum.request({
          method: 'eth_getBalance',
          params: [account, 'latest']
        }).then(function (result) {
          var wei = parseInt(result, 16);
          var balance = wei / Math.pow(10, 18);
          console.log(balance + " MATIC");
          currentUserAtr = currentUserAtr + ':' + balance;
          if (window.unityInstance != null) {
            window.unityInstance.SendMessage('CanvasManager', 'OnEndTransaction', currentUserAtr);
          } //END_IF
        }); //END_REQUEST eth_getBalance
      });
    }); //END_REQUEST eth_sendTransaction
  },

  ConfirmTransaction: function ConfirmTransaction(_amount) {
    alert('Transaction successful' + UTF8ToString(_amount) + ' deposited in your wallet');
  },
  OpenWindow: function OpenWindow(link) {
    var url = UTF8ToString(link);
    document.onpointerup = function () {
      //Use onpointerup for touch input compatibility
      window.open(url);
      document.onpointerup = null;
    };
  }
});