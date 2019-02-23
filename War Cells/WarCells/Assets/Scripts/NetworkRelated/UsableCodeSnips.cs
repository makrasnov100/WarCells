using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UsableCodeSnips : MonoBehaviour
{
    public Text errorText;
    public GameObject PlayerNameForServerOBJ;

    [Header("Panels")]
    public GameObject loginMenu;
    public GameObject registerMenu;

    [Header("LoginInputs")]

    public GameObject lUsernameInput;
    public GameObject lPasswordInput;

    [Header("RegisterInputs")]

    public GameObject rUsernameInput;
    public GameObject rPasswordInput;
    public GameObject rPasswordInputAgain;
    public GameObject rEmailInput;

    private string RegisterURL = "http://fizzostia.tk/InsertUser.php";
    private string LoginURL = "http://fizzostia.tk/Login.php";
    private string dbError;

    // For a user to register their account to the database
    IEnumerator RegisterinToDB(string username, string password, string email)
    {
        WWWForm form = new WWWForm();
        form.AddField("usernamePost", username);
        form.AddField("emailPost", email);
        form.AddField("passwordPost", password);


        WWW www = new WWW(RegisterURL, form);

        yield return www;

        Debug.Log(www.text);
        dbError = www.text;

        // Error handler
        if (dbError == "Everything ok.")
            errorText.text = "Succesful Register!";
        else if (dbError == "there was an error")
            errorText.text = "Username or Email Already in use!";
        else
            errorText.text = "ERROR: Unknown Error";
    }
    // For a user to LOGIN to the game
    IEnumerator LoginToDB(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("usernamePost", username);
        form.AddField("passwordPost", password);

        WWW www = new WWW(LoginURL, form);

        yield return www;

        Debug.Log(www.text);
        dbError = www.text;

        //Error detection
        if (dbError == "user not found")
            errorText.text = "ERROR: Username was not found!";
        else if (dbError == "password incorrect")
            errorText.text = "ERROR: User found, Password incorrect!";
        else if (dbError == "login success")
        {
            errorText.text = "Successful Login";
            //TO-DO Here would be to let the player know its own name for login, and to tell the server on game join
            DontDestroyOnLoad(PlayerNameForServerOBJ);
            PlayerNameForServerOBJ.tag = "PlayerJoinName";
            PlayerNameForServerOBJ.name = username;
            SceneManager.LoadScene("Client");
        }
        else
            errorText.text = "ERROR: Unknown Error";
    }
}
