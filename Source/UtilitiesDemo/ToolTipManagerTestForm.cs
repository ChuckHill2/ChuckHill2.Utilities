//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="ToolTipManagerTestForm.cs" company="Chuck Hill">
// Copyright (c) 2020 Chuck Hill.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 2.1
// of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// The GNU Lesser General Public License can be viewed at
// http://www.opensource.org/licenses/lgpl-license.php. If
// you unfamiliar with this license or have questions about
// it, here is an http://www.gnu.org/licenses/gpl-faq.html.
//
// All code and executables are provided "as is" with no warranty
// either express or implied. The author accepts no liability for
// any damage or loss of business that this product may cause.
// </copyright>
// <repository>https://github.com/ChuckHill2/ChuckHill2.Utilities</repository>
// <author>Chuck Hill</author>
//--------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace UtilitiesDemo
{
    public partial class ToolTipManagerTestForm : Form
    {
        private static readonly string[] Words = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.".Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        private Random rand = new Random(0);

        //private ToolTipManager m_ttToolTipManager;  //must keep object for the lifetime of the form so GC does not attempt to dispose.

        public ToolTipManagerTestForm()
        {
            InitializeComponent();
            //FYI: Property RichTextBox.Rtf not available in Designer!
            m_rtfRichTextBox.Rtf = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Calibri;}}{\colortbl ;\red255\green0\blue0;\red0\green176\blue80;\red0\green77\blue187;}\viewkind4\uc1 \pard\sa200\sl276\slmult1\cf1\b\f0\fs22\lang9 R\cf2 T\cf3 F\cf0\b0  Test\par}";
            SetAllTips(this);  //setup: Recursivly adds tooltip strings to all Control.AccessibleDescription's for AddAllToolTips() default 

            // Note: If creating ToolTipManager() withouot args then must initialize property m_ttToolTipManager.Host = this;
            //m_ttToolTipManager = new ToolTipManager(this);  //Use without Winforms Designer
            //m_ttToolTipManager.UseFading = false;           //disable popup fade
            //m_ttToolTipManager.FillColor = Color.AliceBlue; //change fill color

            m_ttToolTipManager.TipMessageReader += (c) => $"{c.AccessibleDescription} (delegate)";
            m_ttToolTipManager.TipMessageReader += GetTipMessage; //the first delegate above will be replaced.
            m_ttToolTipManager.TipMessageReader += null; //restores back to: (c) => c.AccessibleDescription;

            m_ttToolTipManager.RegisterTooltip(m_llLinkLabel, "This a manually registered tooltip.", nameof(m_llLinkLabel));
            m_ttToolTipManager.RegisterTooltip(m_numNumericUpDown, "This a manually registered\nmulti-line\nmessage.\nMulti-line titles are not allowed.", "m_numNumericUpDown\n(multi-line title)");
            m_ttToolTipManager.CreateRuntimeToolTips();  //RegisterTooltip() always overrides AddAllToolTips()
            m_ttToolTipManager.RegisterTooltip(m_ssStatusStrip, "This is a control but the items within it are not.", nameof(m_ssStatusStrip), ToolTipIcon.Info);
            m_ttToolTipManager.RegisterTooltip(m_msMenuStrip, "This is a control but the items within it are not.", nameof(m_msMenuStrip), ToolTipIcon.Info);
            m_ttToolTipManager.RegisterTooltip(m_tsToolStrip, "This is a control but the items within it are not.", nameof(m_tsToolStrip), ToolTipIcon.Info);

            //m_lblLabel
            //Info
            //Added tooltip from within Designer.
            //Testing...
        }

        private string GetTipMessage(Control c)
        {
            return $"{c.Name}: (custom) {Sentence(rand.Next(0, Words.Length), rand.Next(0, Words.Length))}";
        }

        //Setup: Generate fake AccessibleDescription tooltip messages

        private void SetAllTips(Control cc)
        {
            foreach (Control c in cc.Controls)
            {
                if (string.IsNullOrEmpty(c?.Name)) continue;
                if (c.HasChildren) SetAllTips(c);
                SetTip(c);
            }
        }
        private void SetTip(Control c) => c.AccessibleDescription = $"{c.Name}: (AccessibleDescription) {Sentence(rand.Next(0, Words.Length), rand.Next(0, Words.Length))}";
        private static string Sentence(int indexStart, int indexEnd)
        {
            if (indexEnd < 0) indexEnd = 0;
            if (indexEnd > Words.Length - 1) indexEnd = Words.Length - 1;

            if (indexStart < 0) indexStart = 0;
            if (indexStart > Words.Length - 1) indexStart = Words.Length - 1;

            if (indexStart > indexEnd)
            {
                int i = indexStart;
                indexStart = indexEnd;
                indexEnd = i;
            }

            return string.Join(" ", Words, indexStart, indexEnd - indexStart + 1);
        }

        //Test all the permutations of a tooltip popup

        private class LayoutTest
        {
            public ToolTipIcon Icon;
            public string Title;
            public string Message;
            public LayoutTest(ToolTipIcon icon, string title, string message) { Icon = icon; Title = title; Message = message; }
            public override string ToString() => $"({(Icon == 0 ? "" : Icon.ToString())}) Ttl={Fmt(Title)}, Msg={Fmt(Message)}";
            private static string Fmt(string s) => s == null ? "(null)" : string.Concat("\"", s, "\"");
        }
        private LayoutTest[] _tests = new []
        {
           /* 0*/ new LayoutTest(ToolTipIcon.None,"",""),  //does nothing. no tooltip.
           /* 1*/ new LayoutTest(ToolTipIcon.Info,"",""),  //Icon only

           /* 2*/ new LayoutTest(ToolTipIcon.None,"","No icon + empty title + this message."),
           /* 3*/ new LayoutTest(ToolTipIcon.None,null, "No icon + null title + this message."),
           /* 4*/ new LayoutTest(ToolTipIcon.None,"No icon + this title + empty msg",""),
           /* 5*/ new LayoutTest(ToolTipIcon.None,"No icon + long tooltip title","short msg"),
           /* 6*/ new LayoutTest(ToolTipIcon.None,"No icon + this title","Really long message: " + Sentence(0, 18)),
           /* 7*/ new LayoutTest(ToolTipIcon.None,"No icon + really long title: " + Sentence(0, 18),"Regular tooltip message."),
           /* 8*/ new LayoutTest(ToolTipIcon.None,"No icon + regular Title","Really really long tooltip msg: " + Sentence(0, 100)),

           /* 9*/ new LayoutTest(ToolTipIcon.Info,"","Info icon + empty title + this message."),
           /*10*/ new LayoutTest(ToolTipIcon.Info,null,"Info icon + null title (defaulting to icon name) + this message."),
           /*11*/ new LayoutTest(ToolTipIcon.Warning,"Warning icon + this title + empty msg",""),
           /*12*/ new LayoutTest(ToolTipIcon.Error,"Error icon + long tooltip title","short msg"),
           /*13*/ new LayoutTest(ToolTipIcon.Info,"Info icon + this title","Really long msg: " + Sentence(0, 18)),
           /*14*/ new LayoutTest(ToolTipIcon.Warning,"Warning icon + really long title: " + Sentence(0, 18),"Regular tooltip message."),
           /*15*/ new LayoutTest(ToolTipIcon.Error,"Error icon + regular Title","Really really long tooltip msg: " + Sentence(0, 100)),

           /*16*/ new LayoutTest(ToolTipIcon.Info,"Error icon\n(multi-line)\nregular Title","Multi-line message\nReally really long tooltip msg: " + Sentence(0, 100)),

           /*17*/ new LayoutTest(ToolTipIcon.Info,"","Tooltip Layout Test Complete"),
        };

        private int _index = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            //Button click performs some action. This tooltip is the status of that action.

            //Test different tooltip message permutations.
            Debug.WriteLine($"{_index}. {_tests[_index]}");
            //m_ttToolTipManager?.Show((Control)sender, _tests[_index].Message, _tests[_index].Title, _tests[_index].Icon);
            _index = ++_index % _tests.Length;

            //Test delay, fading, and timeout.
            //m_ttToolTipManager?.Show((Control)sender, $"({_index++}) I've found that if I copy C# code from off of a website, and paste it into Notepad++.", "Title", ToolTipIcon.Info);
        }
    }
}
