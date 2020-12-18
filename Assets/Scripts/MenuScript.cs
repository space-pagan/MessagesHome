// Matan Gazit
// MenuScript.cs
// 
// Associated with menu manager, this screen is attached to all menus
//  in the title scene of the game, allowing a menu to be added to the
//  menu array

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    public string menuName;
    public bool open;

    public void Open()
    {
        open = true;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        open = false;
        gameObject.SetActive(false);
    }
}
