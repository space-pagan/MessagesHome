// Created by Matan Gazit
// MenuManager.cs
//
// Created with the help of Rugbug Redfern tutorials
// https://www.patreon.com/rugbug
//
// Script controls menu behavior in the title menu scene
// Allows for a more streamlined control of turning menus on and off
//  without having to write a seperate method for each menu control scheme

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager MM;
    [SerializeField] MenuScript[] menus;

    void Awake()
    {
        MM = this;
    }

    public void OpenMenu(string menuName)
    {
        for(int i = 0; i < menus.Length; i++)
        {
            if(menus[i].menuName == menuName)
            {
                (menus[i]).Open();
            }
            else if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
    }

    public void OpenMenu(MenuScript menu)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
        menu.Open();
    }

    public void CloseMenu(MenuScript menu)
    {
        menu.Close();
    }

    public void QuitGame()
    {
        Debug.Log("Game is exiting");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
