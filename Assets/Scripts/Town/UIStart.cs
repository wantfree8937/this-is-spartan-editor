using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CharacterInfo
{
    public string Name;
    public int UnlockCost;
    public GameObject LockedImage;
    public GameObject UnlockedImage;

    public CharacterInfo(string name, int unlockCost, GameObject lockedImage, GameObject unlockedImage)
    {
        Name = name;
        UnlockCost = unlockCost;
        LockedImage = lockedImage;
        UnlockedImage = unlockedImage;
    }
}

public class UIStart : MonoBehaviour
{
    [SerializeField] private GameObject charList;
    [SerializeField] private Button[] charBtns;

    [SerializeField] private Button btnConfirm1;
    [SerializeField] private Button btnConfirm2;
    [SerializeField] private Button btnConfirm3;
    [SerializeField] private Button btnRegister;
    [SerializeField] private Button btnSucFail;
    [SerializeField] public  GameObject loginWindow;
    [SerializeField] private GameObject SucFailBox;
    [SerializeField] private TMP_InputField inputServer;
    [SerializeField] private TMP_InputField inputNickname;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private TMP_InputField inputPort;
    [SerializeField] private TMP_Text txtMessage1;
    [SerializeField] private TMP_Text txtMessage2;
    [SerializeField] private TMP_Text txtSucFail;
    [SerializeField] private TMP_Text txtRegisterLogin;
    [SerializeField] private TMP_Text txtloginRegisterTitle;

    [SerializeField] private GameObject userCoinBox;
    [SerializeField] private TMP_Text coinMessage;
    private TMP_Text placeHolder1;
    private TMP_Text placeHolderNickname;
    private TMP_Text placeHolderPassWord;

    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject popupWindow;
    [SerializeField] private TMP_Text popupMessage;
    [SerializeField] private Button popupConfirmButton;
    [SerializeField] private Button popupCancelButton;

    [SerializeField] private GameObject insufficientCoinsPopupWindow;
    [SerializeField] private TMP_Text insufficientCoinsMessage;
    [SerializeField] private Button insufficientCoinsConfirmButton;

    public int classIdx = 0;
    private string serverUrl;
    private string nickname;
    private string password;
    private string port;

    private bool[] isCharacterUnlocked;
    private Dictionary<int, CharacterInfo> characterInfos;
    private Action onPopupConfirm;

    void Start()
    {
        placeHolder1 = inputServer.placeholder.GetComponent<TMP_Text>();
        placeHolderNickname = inputNickname.placeholder.GetComponent<TMP_Text>();
        placeHolderPassWord = inputPassword.placeholder.GetComponent<TMP_Text>();

        InitializeCharacterInfos(new bool[charBtns.Length]); // 기본적으로 모든 캐릭터 잠금
        InitializeCharacterButtons();

        popupConfirmButton.onClick.AddListener(() =>
        {
            onPopupConfirm?.Invoke();
            ClosePopup();
        });

        popupCancelButton.onClick.AddListener(ClosePopup);
        insufficientCoinsConfirmButton.onClick.AddListener(CloseInsufficientCoinsPopup);

        SetServerUI();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return) && inputServer.IsActive())
        {
            btnConfirm1.onClick.Invoke();
        }

        if (Input.GetKeyUp(KeyCode.Return) && inputNickname.IsActive())
        {
            btnConfirm3.onClick.Invoke();
            btnRegister.onClick.Invoke();
        }
    }

    public void InitializeCharacterInfos(bool[] isUnlocked)
    {
        characterInfos = new Dictionary<int, CharacterInfo>
        {
            { 0, new CharacterInfo("케르베", 0, charBtns[0].transform.Find("LockedImage").gameObject, charBtns[0].transform.Find("UnlockedImage").gameObject) },
            { 1, new CharacterInfo("유니", 0, charBtns[1].transform.Find("LockedImage").gameObject, charBtns[1].transform.Find("UnlockedImage").gameObject) },
            { 2, new CharacterInfo("닉스", 0, charBtns[2].transform.Find("LockedImage").gameObject, charBtns[2].transform.Find("UnlockedImage").gameObject) },
            { 3, new CharacterInfo("차드", 100, charBtns[3].transform.Find("LockedImage").gameObject, charBtns[3].transform.Find("UnlockedImage").gameObject) },
            { 4, new CharacterInfo("미호", 100, charBtns[4].transform.Find("LockedImage").gameObject, charBtns[4].transform.Find("UnlockedImage").gameObject) },
            { 5, new CharacterInfo("레비", 300, charBtns[5].transform.Find("LockedImage").gameObject, charBtns[5].transform.Find("UnlockedImage").gameObject) },
            { 6, new CharacterInfo("와이브", 300, charBtns[6].transform.Find("LockedImage").gameObject, charBtns[6].transform.Find("UnlockedImage").gameObject) },
            { 7, new CharacterInfo("드라고", 500, charBtns[7].transform.Find("LockedImage").gameObject, charBtns[7].transform.Find("UnlockedImage").gameObject) },
            { 8, new CharacterInfo("키리", 500, charBtns[8].transform.Find("LockedImage").gameObject, charBtns[8].transform.Find("UnlockedImage").gameObject) }
        };

        isCharacterUnlocked = isUnlocked;

        for (int i = 0; i < charBtns.Length; i++)
        {
            if (isCharacterUnlocked[i])
            {
                characterInfos[i].LockedImage.SetActive(false);
                characterInfos[i].UnlockedImage.SetActive(true);
            }
            else
            {
                characterInfos[i].LockedImage.SetActive(true);
                characterInfos[i].UnlockedImage.SetActive(false);
            }
        }
    }

    private void InitializeCharacterButtons()
    {
        for (int i = 0; i < charBtns.Length; i++)
        {
            int idx = i;
            charBtns[i].onClick.AddListener(() => OnCharacterButtonClick(idx));
        }
    }

    void OnCharacterButtonClick(int idx)
    {
        if (isCharacterUnlocked[idx])
        {
            SelectCharacter(idx);
        }
        else
        {
            ConfirmUnlockCharacter(idx);
        }
    }

    void SelectCharacter(int idx)
    {
        charBtns[classIdx].transform.Find("Border").gameObject.SetActive(false);
        classIdx = idx;
        charBtns[classIdx].transform.Find("Border").gameObject.SetActive(true);
    }

    void ConfirmUnlockCharacter(int idx)
    {
        if (characterInfos.TryGetValue(idx, out CharacterInfo characterInfo))
        {
            ShowPopup(
                $"{characterInfo.Name}을(를) {characterInfo.UnlockCost} 코인으로 잠금 해제하시겠습니까?",
                () =>
                {
                    if (TownManager.Instance.coinDisplay.GetCoinCount() >= characterInfo.UnlockCost)
                    {
                        C_Unlock_Character unlockPacket = new C_Unlock_Character
                        {
                            Nickname = GameManager.Instance.UserName,
                            Class = idx + 1000,
                            Coin = TownManager.Instance.coinDisplay.coinCount,
                        };

                        GameManager.Network.Send(unlockPacket);
                    }
                    else
                    {
                        ShowInsufficientCoinsPopup("코인이 부족합니다!");
                    }
                });
        }
    }

    public void UnlockCharacter(int idx, int coin)
    {
        isCharacterUnlocked[idx] = true;
        charBtns[idx].interactable = true;

        TownManager.Instance.coinDisplay.SetCoins(coin);
        coinMessage.text = $"{coin}";

        if (characterInfos.TryGetValue(idx, out CharacterInfo characterInfo))
        {
            characterInfo.LockedImage.SetActive(false);
            characterInfo.UnlockedImage.SetActive(true);
        }

        SelectCharacter(idx);
    }

    void SetServerUI()
    {
        txtMessage1.color = UnityEngine.Color.white;
        txtMessage1.text = "Welcome!";
        inputServer.text = string.Empty;
        placeHolder1.text = "서버주소를 입력해주세요!";
        charList.SetActive(false);
        loginWindow.gameObject.SetActive(false);
        inputPort.gameObject.SetActive(true);
        btnConfirm1.onClick.RemoveAllListeners();
        btnConfirm1.onClick.AddListener(ConnectServer);
    }

    void SetLoginUI()
    {
        txtMessage2.color = UnityEngine.Color.white;
        txtMessage2.text = "닉네임과 비밀번호를 입력해 주세요";
        txtloginRegisterTitle.color = UnityEngine.Color.white;
        txtloginRegisterTitle.text = "로그인";
        txtRegisterLogin.color = UnityEngine.Color.black;
        txtRegisterLogin.text = "회원가입";

        Image loginImage = loginWindow.GetComponent<Image>();
        if (loginImage != null)
        {
            loginImage.color = new UnityEngine.Color(0.18f, 0.18f, 0.18f);
        }

        inputNickname.text = string.Empty;
        inputPassword.text = string.Empty;
        placeHolderNickname.text = "닉네임을 입력해주세요 (2~10글자)";
        placeHolderPassWord.text = "비밀번호를 입력해주세요 (6자 이상)";
        charList.SetActive(false);
        inputPort.gameObject.SetActive(false);
        btnRegister.onClick.RemoveAllListeners();
        btnRegister.onClick.AddListener(SetRegisterUI);
        btnConfirm3.onClick.RemoveAllListeners();
        btnConfirm3.onClick.AddListener(() => OnConfirmButtonClicked(isLogin: true));
    }

    void SetRegisterUI()
    {
        txtMessage2.color = UnityEngine.Color.white;
        txtMessage2.text = "닉네임과 비밀번호를 입력해 주세요";
        txtloginRegisterTitle.color = UnityEngine.Color.white;
        txtloginRegisterTitle.text = "회원가입";
        txtRegisterLogin.color = UnityEngine.Color.black;
        txtRegisterLogin.text = "로그인";

        Image loginImage = loginWindow.GetComponent<Image>();
        if (loginImage != null)
        {
            loginImage.color = new UnityEngine.Color(0.28f, 0.14f, 0.14f);
        }

        inputNickname.text = string.Empty;
        inputPassword.text = string.Empty;
        placeHolderNickname.text = "닉네임을 입력해주세요 (2~10글자)";
        placeHolderPassWord.text = "비밀번호를 입력해주세요 (6자 이상)";
        charList.SetActive(false);
        inputPort.gameObject.SetActive(false);
        btnRegister.onClick.RemoveAllListeners();
        btnRegister.onClick.AddListener(SetLoginUI);
        btnConfirm3.onClick.RemoveAllListeners();
        btnConfirm3.onClick.AddListener(() => OnConfirmButtonClicked(isLogin: false));
    }

    public void SetCharacterSelectionUI(int coin)
    {
        txtMessage1.color = UnityEngine.Color.white;
        txtMessage1.text = "캐릭터를 선택해주세요";
        charList.SetActive(true);
        loginWindow.gameObject.SetActive(false);
        inputServer.gameObject.SetActive(false);
        inputNickname.gameObject.SetActive(false);
        inputPassword.gameObject.SetActive(false);
        inputPort.gameObject.SetActive(false);
        userCoinBox.SetActive(true);

        coinMessage.text = $"{coin}";

        btnConfirm2.onClick.RemoveAllListeners();
        btnConfirm2.onClick.AddListener(() => StartGame());
    }

    private void OnConfirmButtonClicked(bool isLogin)
    {
        string nickname = inputNickname.text;
        string password = inputPassword.text;

        // 닉네임 및 비밀번호 길이 검사
        if (string.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(password))
        {
            txtMessage2.color = UnityEngine.Color.red;
            txtMessage2.text = "닉네임과 비밀번호를 모두 입력해 주세요";
            return;
        }

        if (nickname.Length < 2 || nickname.Length > 10)
        {
            txtMessage2.color = UnityEngine.Color.red;
            txtMessage2.text = "닉네임은 2~10글자여야 합니다!";
            Debug.Log("UIStart: Nickname length is invalid");
            return;
        }

        if (password.Length < 6)
        {
            txtMessage2.color = UnityEngine.Color.red;
            txtMessage2.text = "비밀번호는 6글자 이상이어야 합니다!";
            Debug.Log("UIStart: Password is too short");
            return;
        }

        if (isLogin)
        {
            // 서버로 로그인 요청을 보내는 로직
            LoginServer(nickname, password);
        }
        else
        {
            // 서버로 회원가입 요청을 보내는 로직
            RegisterServer(nickname, password);
        }
    }

    void LoginServer(string nick, string pw)
    {
       C_Login loginPacket = new C_Login 
       {
           Nickname = nick,
           Password = pw,
       };

       GameManager.Network.Send(loginPacket);
    }

    void RegisterServer(string nick, string pw)
    {
        C_Register registerPacket = new C_Register
        {
            Nickname = nick,
            Password = pw,
        };

        GameManager.Network.Send(registerPacket);
    }

    void ConnectServer()
    {
        GameManager.Network.Init(serverUrl, port);
    }

    public void FailLoginServer() 
    {
        SucFailBox.gameObject.SetActive(true);
        txtSucFail.color = UnityEngine.Color.white;
        txtSucFail.text = "로그인에 실패하였습니다.";

        btnSucFail.onClick.RemoveAllListeners();
        btnSucFail.onClick.AddListener(() =>
        {
            SucFailBox.gameObject.SetActive(false);
        });
    }

    public void FailRegisterServer()
    {
        SucFailBox.gameObject.SetActive(true);
        txtSucFail.color = UnityEngine.Color.white;
        txtSucFail.text = "회원가입에 실패하였습니다.";

        btnSucFail.onClick.RemoveAllListeners();
        btnSucFail.onClick.AddListener(() =>
        {
            SucFailBox.gameObject.SetActive(false);
        });
    }

    public void ConfirmServer(string url, string serverPort)
    {
        serverUrl = url;
        port = serverPort;
        loginWindow.gameObject.SetActive(true);
        SetLoginUI();
    }

    public void ConfirmLogin()
    {
        nickname = inputNickname.text;
        GameManager.Instance.UserName = nickname;
        Debug.Log($"UIStart: Nickname confirmed: {nickname}");
    }

    public void ConfirmRegister()
    {
        SucFailBox.gameObject.SetActive(true);
        txtSucFail.color = UnityEngine.Color.white;
        txtSucFail.text = "회원가입에 성공하였습니다!!";

        btnSucFail.onClick.RemoveAllListeners();
        btnSucFail.onClick.AddListener(() =>
        {
            SucFailBox.gameObject.SetActive(false);
            SetLoginUI();
        });
    }

    public void StartGame(bool deactivateObject = true, int? providedClassIdx = null)
    {
        nickname = GameManager.Instance.UserName;
        classIdx = providedClassIdx ?? classIdx;
        TownManager.Instance.GameStart(serverUrl, port, nickname, classIdx);

        if (deactivateObject)
        {
            gameObject.SetActive(false);
        }
    }

    void ShowInsufficientCoinsPopup(string message)
    {
        insufficientCoinsMessage.text = message;
        insufficientCoinsPopupWindow.SetActive(true);
    }

    void CloseInsufficientCoinsPopup()
    {
        insufficientCoinsPopupWindow.SetActive(false);
    }

    void ShowPopup(string message, Action confirmAction)
    {
        popupMessage.text = message;
        onPopupConfirm = confirmAction;
        popupPanel.SetActive(true);
        popupWindow.SetActive(true);
    }

    void ClosePopup()
    {
        popupPanel.SetActive(false);
        popupWindow.SetActive(false);
    }
}
