using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VbClient.Net;

namespace VbClient
{
    public partial class Form1 : Form
    {
        ClientEvt client = new ClientEvt("222.20.59.63");
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            client.AddFailure += new EventHandler(client_AddFailure);
            client.AddSuccess += new ClientEvt.UpdateMapHandler(client_AddSuccess);
            client.BeginGame += new EventHandler(client_BeginGame);
            client.BeginLoadGame += new EventHandler(client_BeginLoadGame);
            client.CreateFailure += new EventHandler(client_CreateFailure);
            client.CreateSuccess += new EventHandler(client_CreateSuccess);
            client.GetObject += new ClientEvt.UpdateMapHandler(client_GetObject);
            client.GetOtherBike += new ClientEvt.UpdateMapHandler(client_GetOtherBike);
            client.GotTeamMapList += new ClientEvt.TeamMapList(client_GotTeamMapList);
            client.LoginFailure += new EventHandler(client_LoginFailure);
            client.LoginSuccess += new EventHandler(client_LoginSuccess);
            client.SettingMap += new ClientEvt.UpdateMapHandler(client_SettingMap);
        }

        void client_SettingMap(object sender, string map)
        {
            listBox1.Items.Add("SetMap" + map);
        }

        void client_LoginSuccess(object sender, EventArgs e)
        {
            listBox1.Items.Add("LoginSuccess");
        }

        void client_LoginFailure(object sender, EventArgs e)
        {
            listBox1.Items.Add("LoginFailure");
        }

        void client_GotTeamMapList(object sender, List<string> team, List<string> map)
        {
            foreach (string s in team)
                listBox1.Items.Add("---" + s);
            foreach (string s in map)
                listBox1.Items.Add("---" + s);
        }

        void client_GetOtherBike(object sender, string map)
        {

            listBox1.Items.Add("OtherBike" + map);
        }

        void client_GetObject(object sender, string map)
        {
            listBox1.Items.Add("Object" + map);
        }

        void client_CreateSuccess(object sender, EventArgs e)
        {
            listBox1.Items.Add("CreateTeamSucc");
        }

        void client_CreateFailure(object sender, EventArgs e)
        {
            listBox1.Items.Add("CreateTeamFail");
        }

        void client_BeginLoadGame(object sender, EventArgs e)
        {
            listBox1.Items.Add("LoadGame...");
        }

        void client_BeginGame(object sender, EventArgs e)
        {
            listBox1.Items.Add("BeginGame");
        }

        void client_AddSuccess(object sender, string map)
        {
            listBox1.Items.Add("AddSuccess "+map);
        }

        void client_AddFailure(object sender, EventArgs e)
        {
            listBox1.Items.Add("AddFail");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            client.Login("PL", "123");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            client.CreateTeam("race");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            client.GetTeamList();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            client.LeaveTeam("race");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            client.SendBikeState("aaaaaa");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            client.SendObjectState("bbbbb");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            client.SetMap("Forest");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            client.AddTeam("race");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            client.Begin(0);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            client.Begin(2);
        }
    }
}
