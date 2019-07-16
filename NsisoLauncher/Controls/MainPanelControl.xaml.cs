﻿using NsisoLauncher.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using NsisoLauncher.Windows;

namespace NsisoLauncher.Controls
{
    /// <summary>
    /// MainPanelControl.xaml 的交互逻辑
    /// </summary>
    public partial class MainPanelControl : UserControl
    {
        public event Action<LaunchEventArgs> Launch;

        private ObservableCollection<KeyValuePair<string, UserNode>> userList = new ObservableCollection<KeyValuePair<string, UserNode>>();
        private ObservableCollection<KeyValuePair<string, AuthenticationNode>> authNodeList = new ObservableCollection<KeyValuePair<string, AuthenticationNode>>();
        private ObservableCollection<NsisoLauncherCore.Modules.Version> versionList = new ObservableCollection<NsisoLauncherCore.Modules.Version>();

        public MainPanelControl()
        {
            InitializeComponent();
            FirstBinding();
            Refresh();
        }

        private void FirstBinding()
        {
            authTypeCombobox.ItemsSource = authNodeList;
            userComboBox.ItemsSource = userList;
            launchVersionCombobox.ItemsSource = versionList;
        }

        private UserNode GetSelectedAuthNode()
        {
            string userID = (string)userComboBox.SelectedValue;
            if (App.config.MainConfig.User.UserDatabase.ContainsKey(userID))
            {
                return App.config.MainConfig.User.UserDatabase[userID];
            }
            else
            {
                return null;
            }
        }

        public async void Refresh()
        {
            //更新验证模型列表
            authNodeList.Clear();
            authNodeList.Add(new KeyValuePair<string, AuthenticationNode>("offline", new AuthenticationNode()
            {
                AuthType = AuthenticationType.OFFLINE,
                Name = App.GetResourceString("String.MainWindow.Auth.Offline")
            }));
            authNodeList.Add(new KeyValuePair<string, AuthenticationNode>("mojang", new AuthenticationNode()
            {
                AuthType = AuthenticationType.MOJANG,
                Name = App.GetResourceString("String.MainWindow.Auth.Mojang")
            }));
            foreach (var item in App.config.MainConfig.User.AuthenticationDic)
            {
                authNodeList.Add(item);
            }

            //更新用户列表
            userList.Clear();
            foreach (var item in App.config.MainConfig.User.UserDatabase)
            {
                userList.Add(item);
            }
            this.userComboBox.SelectedValue = App.config.MainConfig.History.SelectedUserNodeID;

            //更新版本列表
            versionList.Clear();
            List<NsisoLauncherCore.Modules.Version> versions = await App.handler.GetVersionsAsync();
            foreach (var item in versions)
            {
                versionList.Add(item);
            }
            this.launchVersionCombobox.Text = App.config.MainConfig.History.LastLaunchVersion;

            App.logHandler.AppendDebug("启动器主窗体数据重载完毕");
        }

        public async void RefreshIcon()
        {
            //头像自定义显示皮肤
            UserNode node = GetSelectedAuthNode();
            if (node == null)
            {
                return;
            }
            bool isNeedRefreshIcon = (!string.IsNullOrWhiteSpace(node.SelectProfileUUID)) &&
                node.AuthModule == "mojang";
            if (isNeedRefreshIcon)
            {
                await headScul.RefreshIcon(node.SelectProfileUUID);
            }
        }

        #region button click event

        //启动游戏按钮点击
        private void launchButton_Click(object sender, RoutedEventArgs e)
        {
            //获取启动版本
            NsisoLauncherCore.Modules.Version launchVersion = null;
            if (launchVersionCombobox.SelectedItem != null)
            {
                launchVersion = (NsisoLauncherCore.Modules.Version)launchVersionCombobox.SelectedItem;
            }

            //获取验证方式
            AuthenticationNode authNode = null;
            string authNodeName = null;
            if (authTypeCombobox.SelectedItem != null)
            {
                KeyValuePair<string, AuthenticationNode> node = (KeyValuePair<string, AuthenticationNode>)authTypeCombobox.SelectedItem;
                authNode = node.Value;
                authNodeName = node.Key;
            }

            //获取用户信息
            string userName = userComboBox.Text;
            string selectedUserUUID = (string)userComboBox.SelectedValue;
            bool isNewUser = string.IsNullOrWhiteSpace(selectedUserUUID);
            UserNode userNode = null;
            if (!string.IsNullOrWhiteSpace(userName))
            {
                if (!isNewUser)
                {
                    userNode = ((KeyValuePair<string, UserNode>)userComboBox.SelectedItem).Value;
                }
                else
                {
                    userNode = new UserNode() { AuthModule = authNodeName, UserName = userName };
                }
            }
            else
            {
                userNode = null;
            }
            

            this.Launch?.Invoke(new LaunchEventArgs() { AuthNode = authNode, UserNode = userNode, LaunchVersion = launchVersion, IsNewUser = isNewUser});
        }

        //下载按钮点击
        private void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            new DownloadWindow().ShowDialog();
            Refresh();
        }

        //配置按钮点击
        private void configButton_Click(object sender, RoutedEventArgs e)
        {
            new SettingWindow().ShowDialog();
            Refresh();
            //CustomizeRefresh();
        }
        #endregion

    }

    public class LaunchEventArgs : EventArgs
    {
        public NsisoLauncherCore.Modules.Version LaunchVersion { get; set; }
        public AuthenticationNode AuthNode { get; set; }
        public UserNode UserNode { get; set; }
        public bool IsNewUser { get; set; }
    }

    public enum LaunchType
    {
        NORMAL,
        SAFE,
        CREATE_SHORT
    }
}