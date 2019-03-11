using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace RandomHill3
{
    public partial class Form1 : Form
    {
        //sh3
        int dogPercent = 100; // 0x200 0x8
        int numbPercent = 100; // 0x201
        int numbSmallPercent = 100; // 0x5
        int numbMedPercent = 100; // 0x6
        int numbLargePercent = 100; // 0x7
        int closerPercent = 100; // 0x202
        int closerNormalPercent = 100; // 0x1
        int closerLatePercent = 0; // 0x2
        int closerDeadPercent = 0; // 0x3
        int closerRisingPercent = 0; // 0x4
        int nursePercent = 100; // 0x203
        int nursePipePercent = 100; // 0xB
        int nurseGunPercent = 100; // 0xC
        int nurseJerkPipePercent = 0; // 0xD
        int nurseJerkGunPercent = 0; // 0xE
        int snorlaxPercent = 100; // 0x204
        int snorlaxSitPercent = 100; // 0x9
        int snorlaxSlpPercent = 100; // 0xA
        int pendulumPercent = 100; // 0x205
        int pendulumBWPercent = 100; // 0xF
        int pendulumBFPercent = 100; // 0x10
        int pendulumSWPercent = 100; // 0x11
        int pendulumSFPercent = 100; // 0x12
        int whiteGatorPercent = 50; // 0x20B
        int brownGatorPercent = 50; // 0x20A
        int brownGatorNormalPercent = 100; // 0x16
        int brownGatorDeadPercent = 0; // 0x17
        int brownGatorFakingPercent = 100; // 0x18
        int scraperPercent = 100; // 0x206
        int missionaryPercent = 100; // 0x211
        int leonardPercent = 100; // 0x213
        int godPercent = 0; // 0x214
        int alessaPercent = 100; // 0x215

        bool randomiseEnemies = true;
        bool invisibleEnemies = false;
        bool randoSHtown = false;

        public struct SH3_Probs
        {
            public short id;
            public short d;
            public SH3_Probs(short idp, short dp) { id = idp; d = dp; }
        }
        int sh3_probsMax = 99;
        SH3_Probs[] sh3_probs = new SH3_Probs[100];

        Process sh3_proc = null;
        bool sh3_processHooked = false;

        //sh2
        int sh2_figurePercent = 100; // 0x200
        int sh2_manekinPercent = 100; // 0x201
        int sh2_bugPercent = 100; // 0x202
        int sh2_nursePercent = 50; // 0x207
        int sh2_phPercent = 100; // 0x208
        int sh2_darkNursePercent = 100; // 0x20B

        public struct SH2_Probs
        {
            public short id;
            public SH2_Probs(short idp) { id = idp; }
        }
        int sh2_probsMax = 99;
        SH2_Probs[] sh2_probs = new SH2_Probs[100];

        Process sh2_proc = null;
        bool sh2_processHooked = false;
        bool lookforsh2 = false;

        public Form1()
        {
            InitializeComponent();
            UpdateDisplays();
            Timer sh3_timer = new Timer();
            sh3_timer.Interval = 1;
            sh3_timer.Tick += new EventHandler(sh3_timer_Tick);
            sh3_timer.Start();

            Timer sh2_timer = new Timer();
            sh2_timer.Interval = 1;
            sh2_timer.Tick += new EventHandler(sh2_timer_Tick);
            sh2_timer.Start();
        }

        private void sh3_timer_Tick(object sender, EventArgs e)
        {
            if (lookforsh2)
            {
                Process[] ps = Process.GetProcessesByName("sh2pc");
                if (ps.Length > 0)
                {
                    sh2_proc = ps[0];
                    lookforsh2 = false; ;
                }
            }
            if (sh3_proc != null && sh3_proc.HasExited) sh3_proc = null;
            if (!sh3_processHooked)
            {
                if (sh3_proc != null)
                {
                    if (sh3_proc.MainWindowHandle != null)
                    {
                        FuckWithSH3Process();
                    }
                }
            }
            else
            {
                if (sh3_proc == null)
                {
                    sh3_processHooked = false;
                    btn_play.Enabled = true;
                    btn_lucky.Enabled = true;
                    tss_status.Text = "Ready";
                }
                else
                {
                    textbox_ingametime.Text = TimeSpan.FromSeconds(Scribe.ReadSingle(new IntPtr(0x070E66F4))).ToString("hh':'mm':'ss");
                    textbox_highscore.Text = Scribe.ReadInt32(new IntPtr(0x070E66F0)).ToString();
                    textbox_killchain.Text = Scribe.ReadSingle(new IntPtr(0x0712C5A0)).ToString();
                    textbox_bonus.Text = Scribe.ReadSingle(new IntPtr(0x0712C59C)).ToString();
                }
            }
        }

        private void sh2_timer_Tick(object sender, EventArgs e)
        {
            if (sh2_proc != null && sh2_proc.HasExited) sh2_proc = null;
            if (!sh2_processHooked)
            {
                if (sh2_proc != null)
                {
                    if (sh2_proc.MainWindowHandle != null)
                    {
                        FuckWithSH2Process();
                    }
                }
            }
            else
            {
                if (sh2_proc == null)
                {
                    sh2_processHooked = false;
                    btn_sh2_play.Enabled = true;
                    btn_sh2_lucky.Enabled = true;
                    tss_status.Text = "Ready";
                }
            }
        }

        private void SetFinalSH3Probs(int mainValue, TextBox tb, short typeID, 
            TextBox opt1 = null, int opt1Val = 0, short opt1D = 0, 
            TextBox opt2 = null, int opt2Val = 0, short opt2D = 0, 
            TextBox opt3 = null, int opt3Val = 0, short opt3D = 0, 
            TextBox opt4 = null, int opt4Val = 0, short opt4D = 0)
        {
            int total = GetSH3Total();
            int optTotal = opt1Val + opt2Val + opt3Val + opt4Val;
            if (tb != null) tb.Text = ((99.0f * (float)mainValue) / (float)total).ToString("0.##") + "%";
            if (opt1 != null) opt1.Text = ((100.0f * (float)opt1Val) / (float)optTotal).ToString("0.##") + "%";
            if (opt2 != null) opt2.Text = ((100.0f * (float)opt2Val) / (float)optTotal).ToString("0.##") + "%";
            if (opt3 != null) opt3.Text = ((100.0f * (float)opt3Val) / (float)optTotal).ToString("0.##") + "%";
            if (opt4 != null) opt4.Text = ((100.0f * (float)opt4Val) / (float)optTotal).ToString("0.##") + "%";

            int locTot = (99 * mainValue) / total;
            for (int j = 0; j != (locTot * opt1Val) / optTotal; j++)
            {
                sh3_probs[sh3_probsMax++] = new SH3_Probs(typeID, opt1D);
            }
            for (int j = 0; j != (locTot * opt2Val) / optTotal; j++)
            {
                sh3_probs[sh3_probsMax++] = new SH3_Probs(typeID, opt2D);
            }
            for (int j = 0; j != (locTot * opt3Val) / optTotal; j++)
            {
                sh3_probs[sh3_probsMax++] = new SH3_Probs(typeID, opt3D);
            }
            for (int j = 0; j != (locTot * opt4Val) / optTotal; j++)
            {
                sh3_probs[sh3_probsMax++] = new SH3_Probs(typeID, opt4D);
            }
        }

        private void SetFinalSH2Probs(int mainValue, TextBox tb, short typeID)
        {
            int total = GetSH2Total();
            if (tb != null) tb.Text = ((100.0f * (float)mainValue) / (float)total).ToString("0.##") + "%";
            
            for (int j = 0; j != (100 * mainValue) / total; j++)
            {
                sh2_probs[sh2_probsMax++] = new SH2_Probs(typeID);
            }
        }

        private int GetSH3Total()
        {
            return dogPercent + numbPercent + closerPercent + nursePercent +
                snorlaxPercent + pendulumPercent + whiteGatorPercent + brownGatorPercent +
                scraperPercent + missionaryPercent + leonardPercent + godPercent + alessaPercent;
        }

        private int GetSH2Total()
        {
            return sh2_figurePercent + sh2_manekinPercent + sh2_bugPercent + sh2_nursePercent + 
                sh2_phPercent + sh2_darkNursePercent;
        }

        private void UpdateDisplays()
        {
            sld_dog.Value = dogPercent;
            norm_dog.Text = dogPercent.ToString();

            sld_numb.Value = numbPercent;
            norm_numb.Text = numbPercent.ToString();
            sld_numb_s.Value = numbSmallPercent;
            norm_numb_s.Text = numbSmallPercent.ToString();
            sld_numb_m.Value = numbMedPercent;
            norm_numb_m.Text = numbMedPercent.ToString();
            sld_numb_l.Value = numbLargePercent;
            norm_numb_l.Text = numbLargePercent.ToString();

            sld_closer.Value = closerPercent;
            norm_closer.Text = closerPercent.ToString();
            sld_closer_normal.Value = closerNormalPercent;
            norm_closer_normal.Text = closerNormalPercent.ToString();
            sld_closer_late.Value = closerLatePercent;
            norm_closer_late.Text = closerLatePercent.ToString();
            sld_closer_dead.Value = closerDeadPercent;
            norm_closer_dead.Text = closerDeadPercent.ToString();
            sld_closer_rising.Value = closerRisingPercent;
            norm_closer_rising.Text = closerRisingPercent.ToString();

            sld_nurse.Value = nursePercent;
            norm_nurse.Text = nursePercent.ToString();
            sld_nurse_pipe.Value = nursePipePercent;
            norm_nurse_pipe.Text = nursePipePercent.ToString();
            sld_nurse_gun.Value = nurseGunPercent;
            norm_nurse_gun.Text = nurseGunPercent.ToString();
            sld_nurse_jerkpipe.Value = nurseJerkPipePercent;
            norm_nurse_jerkpipe.Text = nurseJerkPipePercent.ToString();
            sld_nurse_jerkgun.Value = nurseJerkGunPercent;
            norm_nurse_jerkgun.Text = nurseJerkGunPercent.ToString();

            sld_snorlax.Value = snorlaxPercent;
            norm_snorlax.Text = snorlaxPercent.ToString();
            sld_snorlax_sit.Value = snorlaxSitPercent;
            norm_snorlax_sit.Text = snorlaxSitPercent.ToString();
            sld_snorlax_slp.Value = snorlaxSlpPercent;
            norm_snorlax_slp.Text = snorlaxSlpPercent.ToString();

            sld_pendulum.Value = pendulumPercent;
            norm_pendulum.Text = pendulumPercent.ToString();
            sld_pendulum_bigwalk.Value = pendulumBWPercent;
            norm_pendulum_bigwalk.Text = pendulumBWPercent.ToString();
            sld_pendulum_bigfly.Value = pendulumBFPercent;
            norm_pendulum_bigfly.Text = pendulumBFPercent.ToString();
            sld_pendulum_smallwalk.Value = pendulumSWPercent;
            norm_pendulum_smallwalk.Text = pendulumSWPercent.ToString();
            sld_pendulum_smallfly.Value = pendulumSFPercent;
            norm_pendulum_smallfly.Text = pendulumSFPercent.ToString();

            sld_whiteSlurper.Value = whiteGatorPercent;
            norm_whiteSlurper.Text = whiteGatorPercent.ToString();

            sld_brownSlurper.Value = brownGatorPercent;
            norm_brownSlurper.Text = brownGatorPercent.ToString();
            sld_brownSlurper_normal.Value = brownGatorNormalPercent;
            norm_brownSlurper_normal.Text = brownGatorNormalPercent.ToString();
            sld_brownSlurper_dead.Value = brownGatorDeadPercent;
            norm_brownSlurper_dead.Text = brownGatorDeadPercent.ToString();
            sld_brownSlurper_faking.Value = brownGatorFakingPercent;
            norm_brownSlurper_faking.Text = brownGatorFakingPercent.ToString();

            sld_scraper.Value = scraperPercent;
            norm_scraper.Text = scraperPercent.ToString();

            sld_missionary.Value = missionaryPercent;
            norm_missionary.Text = missionaryPercent.ToString();

            sld_leonard.Value = leonardPercent;
            norm_leonard.Text = leonardPercent.ToString();

            sld_god.Value = godPercent;
            norm_god.Text = godPercent.ToString();

            sld_alessa.Value = alessaPercent;
            norm_alessa.Text = alessaPercent.ToString();
            
            sh3_probsMax = 0;
            SetFinalSH3Probs(dogPercent, perc_dog, 0x200, 
                null, 100, 0x8);
            SetFinalSH3Probs(numbPercent, perc_numb, 0x201, 
                perc_numb_s, numbSmallPercent, 0x5,
                perc_numb_m, numbMedPercent, 0x6,
                perc_numb_l, numbLargePercent, 0x7);
            SetFinalSH3Probs(closerPercent, perc_closer, 0x202,
                perc_closer_normal, closerNormalPercent, 0x1,
                perc_closer_late, closerLatePercent, 0x2,
                perc_closer_dead, closerDeadPercent, 0x3,
                perc_closer_rising, closerRisingPercent, 0x4);
            SetFinalSH3Probs(nursePercent, perc_nurse, 0x203, 
                perc_nurse_pipe, nursePipePercent, 0xB,
                perc_nurse_gun, nurseGunPercent, 0xC,
                perc_nurse_jerkpipe, nurseJerkPipePercent, 0xD,
                perc_nurse_jerkgun, nurseJerkGunPercent, 0xE);
            SetFinalSH3Probs(snorlaxPercent, perc_snorlax, 0x204, 
                perc_snorlax_sit, snorlaxSitPercent, 0x9,
                perc_snorlax_sleep, snorlaxSlpPercent, 0xA);
            SetFinalSH3Probs(pendulumPercent, perc_pendulum, 0x205, 
                perc_pendulum__bigwalk, pendulumBWPercent, 0xF,
                perc_pendulum__bigfly, pendulumBFPercent, 0x10,
                perc_pendulum__smallwalk, pendulumSWPercent, 0x11,
                perc_pendulum__smallfly, pendulumSFPercent, 0x12);
            SetFinalSH3Probs(scraperPercent, perc_scraper, 0x206, 
                null, 100, 0);
            SetFinalSH3Probs(brownGatorPercent, perc_brownSlurper, 0x20A, 
                perc_brownSlurper_normal, brownGatorNormalPercent, 0x16,
                perc_brownSlurper_dead, brownGatorDeadPercent, 0x17,
                perc_brownSlurper_faking, brownGatorFakingPercent, 0x18);
            SetFinalSH3Probs(whiteGatorPercent, perc_whiteSlurper, 0x20B, 
                null, 100, 0);
            SetFinalSH3Probs(missionaryPercent, perc_missionary, 0x211, 
                null, 100, 0);
            SetFinalSH3Probs(leonardPercent, perc_leonard, 0x213, 
                null, 100, 0);
            SetFinalSH3Probs(godPercent, perc_god, 0x214, 
                null, 100, 0);
            SetFinalSH3Probs(alessaPercent, perc_alessa, 0x215, 
                null, 100, 0);
            sh3_probs[sh3_probsMax++] = new SH3_Probs(0x209, 0x0);


            sld_sh2_figure.Value = sh2_figurePercent;
            norm_sh2_figure.Text = sh2_figurePercent.ToString();

            sld_sh2_manekin.Value = sh2_manekinPercent;
            norm_sh2_manekin.Text = sh2_manekinPercent.ToString();

            sld_sh2_creeper.Value = sh2_bugPercent;
            norm_sh2_creeper.Text = sh2_bugPercent.ToString();

            sld_sh2_darknurse.Value = sh2_darkNursePercent;
            norm_sh2_darknurse.Text = sh2_darkNursePercent.ToString();

            sld_sh2_nurse.Value = sh2_nursePercent;
            norm_sh2_nurse.Text = sh2_nursePercent.ToString();

            sld_sh2_ph.Value = sh2_phPercent;
            norm_sh2_ph.Text = sh2_phPercent.ToString();

            sh2_probsMax = 0;
            SetFinalSH2Probs(sh2_figurePercent, perc_sh2_figure, 0x200);
            SetFinalSH2Probs(sh2_manekinPercent, perc_sh2_manekin, 0x201);
            SetFinalSH2Probs(sh2_bugPercent, perc_sh2_creeper, 0x202);
            SetFinalSH2Probs(sh2_darkNursePercent, perc_sh2_darknurse, 0x20B);
            SetFinalSH2Probs(sh2_nursePercent, perc_sh2_nurse, 0x207);
            SetFinalSH2Probs(sh2_phPercent, perc_sh2_ph, 0x208);
        }

        private void OnMainSH3SliderChanged(TrackBar tb, ref int mainValue)
        {
            mainValue = tb.Value;
            if (GetSH3Total() == 0) mainValue = 1;
            UpdateDisplays();
        }

        private void OnMainSH2SliderChanged(TrackBar tb, ref int mainValue)
        {
            mainValue = tb.Value;
            if (GetSH2Total() == 0) mainValue = 1;
            UpdateDisplays();
        }

        private void OnOptionSliderChanged(TrackBar tb, ref int mainValue, int opt2, int opt3, int opt4)
        {
            mainValue = tb.Value;
            if (mainValue + opt2 + opt3 + opt4 == 0) mainValue = 1;
            UpdateDisplays();
        }

        private void sld_dog_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH3SliderChanged(sld_dog, ref dogPercent);
        }

        private void sld_numb_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH3SliderChanged(sld_numb, ref numbPercent);
        }

        private void sld_numb_s_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_numb_s, ref numbSmallPercent, numbMedPercent, numbLargePercent, 0);
        }

        private void sld_numb_m_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_numb_m, ref numbMedPercent, numbSmallPercent, numbLargePercent, 0);
        }

        private void sld_numb_l_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_numb_l, ref numbLargePercent, numbMedPercent, numbSmallPercent, 0);
        }

        private void sld_closer_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH3SliderChanged(sld_closer, ref closerPercent);
        }

        private void sld_closer_normal_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_closer_normal, ref closerNormalPercent, closerLatePercent, closerDeadPercent, closerRisingPercent);
        }

        private void sld_closer_late_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_closer_late, ref closerLatePercent, closerNormalPercent, closerDeadPercent, closerRisingPercent);
        }

        private void sld_closer_dead_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_closer_dead, ref closerDeadPercent, closerLatePercent, closerNormalPercent, closerRisingPercent);
        }

        private void sld_closer_rising_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_closer_rising, ref closerRisingPercent, closerLatePercent, closerDeadPercent, closerNormalPercent);
        }

        private void sld_nurse_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH3SliderChanged(sld_nurse, ref nursePercent);
        }

        private void sld_nurse_pipe_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_nurse_pipe, ref nursePipePercent, nurseGunPercent, nurseJerkPipePercent, nurseJerkGunPercent);
        }

        private void sld_nurse_gun_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_nurse_gun, ref nurseGunPercent, nursePipePercent, nurseJerkPipePercent, nurseJerkGunPercent);
        }

        private void sld_nurse_jerkpipe_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_nurse_jerkpipe, ref nurseJerkPipePercent, nurseGunPercent, nursePipePercent, nurseJerkGunPercent);
        }

        private void sld_nurse_jerkgun_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_nurse_jerkgun, ref nurseJerkGunPercent, nurseGunPercent, nurseJerkPipePercent, nursePipePercent);
        }

        private void sld_snorlax_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH3SliderChanged(sld_snorlax, ref snorlaxPercent);
        }

        private void sld_snorlax_sit_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_snorlax_sit, ref snorlaxSitPercent, snorlaxSlpPercent, 0, 0);
        }

        private void sld_snorlax_slp_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_snorlax_slp, ref snorlaxSlpPercent, snorlaxSitPercent, 0, 0);
        }

        private void sld_pendulum_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH3SliderChanged(sld_pendulum, ref pendulumPercent);
        }

        private void sld_pendulum_bigwalk_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_pendulum_bigwalk, ref pendulumBWPercent, pendulumBFPercent, pendulumSWPercent, pendulumSFPercent);
        }

        private void sld_pendulum_bigfly_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_pendulum_bigfly, ref pendulumBFPercent, pendulumBWPercent, pendulumSWPercent, pendulumSFPercent);
        }

        private void sld_pendulum_smallwalk_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_pendulum_smallwalk, ref pendulumSWPercent, pendulumBFPercent, pendulumBWPercent, pendulumSFPercent);
        }

        private void sld_pendulum_smallfly_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_pendulum_smallfly, ref pendulumSFPercent, pendulumBFPercent, pendulumSWPercent, pendulumBWPercent);
        }

        private void sld_whiteSlurper_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH3SliderChanged(sld_whiteSlurper, ref whiteGatorPercent);
        }

        private void sld_brownSlurper_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH3SliderChanged(sld_brownSlurper, ref brownGatorPercent);
        }

        private void sld_brownSlurper_normal_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_brownSlurper_normal, ref brownGatorNormalPercent, brownGatorDeadPercent, brownGatorFakingPercent, 0);
        }

        private void sld_brownSlurper_dead_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_brownSlurper_dead, ref brownGatorDeadPercent, brownGatorNormalPercent, brownGatorFakingPercent, 0);
        }

        private void sld_brownSlurper_faking_ValueChanged(object sender, EventArgs e)
        {
            OnOptionSliderChanged(sld_brownSlurper_faking, ref brownGatorFakingPercent, brownGatorDeadPercent, brownGatorNormalPercent, 0);
        }

        private void sld_scraper_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH3SliderChanged(sld_scraper, ref scraperPercent);
        }

        private void sld_missionary_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH3SliderChanged(sld_missionary, ref missionaryPercent);
        }

        private void sld_leonard_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH3SliderChanged(sld_leonard, ref leonardPercent);
        }

        private void sld_god_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH3SliderChanged(sld_god, ref godPercent);
        }

        private void sld_alessa_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH3SliderChanged(sld_alessa, ref alessaPercent);
        }

        private void OnMainSH3TextChanged(TextBox tb, ref int mainValue)
        {
            int value = 0;
            if (Int32.TryParse(tb.Text, out value))
            {
                if (value > 100) mainValue = 100;
                else if (value < 0) mainValue = 0;
                mainValue = value;
            }
            if (GetSH3Total() == 0) mainValue = 1;
            UpdateDisplays();
        }

        private void OnMainSH2TextChanged(TextBox tb, ref int mainValue)
        {
            int value = 0;
            if (Int32.TryParse(tb.Text, out value))
            {
                if (value > 100) mainValue = 100;
                else if (value < 0) mainValue = 0;
                mainValue = value;
            }
            if (GetSH2Total() == 0) mainValue = 1;
            UpdateDisplays();
        }

        private void OnOptionTextChanged(TextBox tb, ref int mainValue, int opt2, int opt3, int opt4)
        {
            int value = 0;
            if (Int32.TryParse(tb.Text, out value))
            {
                if (value > 100) mainValue = 100;
                else if (value < 0) mainValue = 0;
                mainValue = value;
            }
            if (mainValue + opt2 + opt3 + opt4 == 0) mainValue = 1;
            UpdateDisplays();
        }

        private void norm_dog_TextChanged(object sender, EventArgs e)
        {
            OnMainSH3TextChanged(norm_dog, ref dogPercent);
        }

        private void norm_numb_TextChanged(object sender, EventArgs e)
        {
            OnMainSH3TextChanged(norm_numb, ref numbPercent);
        }

        private void norm_numb_s_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_numb_s, ref numbSmallPercent, numbMedPercent, numbLargePercent, 0);
        }

        private void norm_numb_m_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_numb_m, ref numbMedPercent, numbSmallPercent, numbLargePercent, 0);
        }

        private void norm_numb_l_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_numb_l, ref numbLargePercent, numbMedPercent, numbSmallPercent, 0);
        }

        private void norm_closer_TextChanged(object sender, EventArgs e)
        {
            OnMainSH3TextChanged(norm_closer, ref closerPercent);
        }
        private void norm_closer_normal_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_closer_normal, ref closerNormalPercent, closerLatePercent, closerDeadPercent, closerRisingPercent);
        }

        private void norm_closer_late_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_closer_late, ref closerLatePercent, closerNormalPercent, closerDeadPercent, closerRisingPercent);
        }

        private void norm_closer_dead_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_closer_dead, ref closerDeadPercent, closerLatePercent, closerNormalPercent, closerRisingPercent);
        }

        private void norm_closer_rising_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_closer_rising, ref closerRisingPercent, closerLatePercent, closerDeadPercent, closerNormalPercent);
        }

        private void norm_nurse_TextChanged(object sender, EventArgs e)
        {
            OnMainSH3TextChanged(norm_nurse, ref nursePercent);
        }

        private void norm_nurse_pipe_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_nurse_pipe, ref nursePipePercent, nurseGunPercent, nurseJerkPipePercent, nurseJerkGunPercent);
        }

        private void norm_nurse_gun_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_nurse_gun, ref nurseGunPercent, nursePipePercent, nurseJerkPipePercent, nurseJerkGunPercent);
        }

        private void norm_nurse_jerkpipe_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_nurse_jerkpipe, ref nurseJerkPipePercent, nursePipePercent, nurseGunPercent, nurseJerkGunPercent);
        }

        private void norm_nurse_jerkgun_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_nurse_jerkgun, ref nurseJerkGunPercent, nursePipePercent, nurseGunPercent, nurseJerkPipePercent);
        }

        private void norm_snorlax_TextChanged(object sender, EventArgs e)
        {
            OnMainSH3TextChanged(norm_snorlax, ref snorlaxPercent);
        }

        private void norm_snorlax_sit_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_snorlax_sit, ref snorlaxSitPercent, snorlaxSlpPercent, 0, 0);
        }

        private void norm_snorlax_slp_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_snorlax_slp, ref snorlaxSlpPercent, snorlaxSitPercent, 0, 0);
        }

        private void norm_pendulum_TextChanged(object sender, EventArgs e)
        {
            OnMainSH3TextChanged(norm_pendulum, ref pendulumPercent);
        }

        private void norm_pendulum_bigwalk_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_pendulum_bigwalk, ref pendulumBWPercent, pendulumBFPercent, pendulumSWPercent, pendulumSFPercent);
        }

        private void norm_pendulum_bigfly_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_pendulum_bigfly, ref pendulumBFPercent, pendulumBWPercent, pendulumSWPercent, pendulumSFPercent);
        }

        private void norm_pendulum_smallwalk_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_pendulum_smallwalk, ref pendulumSWPercent, pendulumBFPercent, pendulumBWPercent, pendulumSFPercent);
        }

        private void norm_pendulum_smallfly_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_pendulum_smallfly, ref pendulumSFPercent, pendulumBFPercent, pendulumSWPercent, pendulumBWPercent);
        }

        private void norm_whiteSlurper_TextChanged(object sender, EventArgs e)
        {
            OnMainSH3TextChanged(norm_whiteSlurper, ref whiteGatorPercent);
        }

        private void norm_brownSlurper_TextChanged(object sender, EventArgs e)
        {
            OnMainSH3TextChanged(norm_brownSlurper, ref brownGatorPercent);
        }

        private void norm_brownSlurper_normal_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_brownSlurper_normal, ref brownGatorNormalPercent, brownGatorDeadPercent, brownGatorFakingPercent, 0);
        }
        private void norm_brownSlurper_dead_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_brownSlurper_dead, ref brownGatorDeadPercent, brownGatorFakingPercent, brownGatorNormalPercent, 0);
        }

        private void norm_brownSlurper_faking_TextChanged(object sender, EventArgs e)
        {
            OnOptionTextChanged(norm_brownSlurper_faking, ref brownGatorFakingPercent, brownGatorDeadPercent, brownGatorNormalPercent, 0);
        }

        private void norm_scraper_TextChanged(object sender, EventArgs e)
        {
            OnMainSH3TextChanged(norm_scraper, ref scraperPercent);
        }

        private void norm_missionary_TextChanged(object sender, EventArgs e)
        {
            OnMainSH3TextChanged(norm_missionary, ref missionaryPercent);
        }

        private void norm_leonard_TextChanged(object sender, EventArgs e)
        {
            OnMainSH3TextChanged(norm_leonard, ref leonardPercent);
        }
        private void norm_god_TextChanged(object sender, EventArgs e)
        {
            OnMainSH3TextChanged(norm_god, ref godPercent);
        }

        private void norm_alessa_TextChanged(object sender, EventArgs e)
        {
            OnMainSH3TextChanged(norm_alessa, ref alessaPercent);
        }

        private void ofd_pathDialog_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbx_sh3path.Text = openFileDialog1.FileName;
            }
        }

        private void tbx_sh3path_TextChanged(object sender, EventArgs e)
        {
            if (!sh3_processHooked)
            {
                if (File.Exists(tbx_sh3path.Text) && Path.GetFileName(tbx_sh3path.Text) == "sh3.exe")
                {
                    btn_play.Enabled = true;
                    btn_lucky.Enabled = true;
                    tss_status.Text = "Ready";
                }
                else
                {
                    btn_play.Enabled = false;
                    btn_lucky.Enabled = false;
                    tss_status.Text = "Need exe";
                }
            }
        }

        private void btn_play_Click(object sender, EventArgs e)
        {
            playSH3();
        }

        private void btn_lucky_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            sh3_probsMax = 0;
            SetFinalSH3Probs(r.Next(1, 101), null, 0x200,
                null, 100, 0x8);
            SetFinalSH3Probs(r.Next(1, 101), null, 0x201,
                null, r.Next(1, 101), 0x5,
                null, r.Next(1, 101), 0x6,
                null, r.Next(1, 101), 0x7);
            SetFinalSH3Probs(r.Next(1, 101), null, 0x202,
                null, r.Next(1, 101), 0x1,
                null, 0, 0x2,
                null, 0, 0x3,
                null, 0, 0x4);
            SetFinalSH3Probs(r.Next(1, 101), null, 0x203,
                null, r.Next(1, 101), 0xB,
                null, r.Next(1, 101), 0xC,
                null, 0, 0xD,
                null, 0, 0xE);
            SetFinalSH3Probs(r.Next(1, 101), null, 0x204,
                null, r.Next(1, 101), 0x9,
                null, r.Next(1, 101), 0xA);
            SetFinalSH3Probs(r.Next(1, 101), null, 0x205,
                null, r.Next(1, 101), 0xF,
                null, r.Next(1, 101), 0x10,
                null, r.Next(1, 101), 0x11,
                null, r.Next(1, 101), 0x12);
            SetFinalSH3Probs(r.Next(1, 101), null, 0x206,
                null, 100, 0);
            SetFinalSH3Probs(r.Next(1, 101), null, 0x20A,
                null, r.Next(1, 101), 0x16,
                null, 0, 0x17,
                null, 0, 0x18);
            SetFinalSH3Probs(r.Next(1, 101), null, 0x20B,
                null, 100, 0);
            SetFinalSH3Probs(r.Next(1, 101), null, 0x211,
                null, 100, 0);
            SetFinalSH3Probs(r.Next(1, 101), null, 0x213,
                null, 100, 0);
            SetFinalSH3Probs(0, perc_god, 0x214,
                null, 100, 0);
            SetFinalSH3Probs(r.Next(1, 101), null, 0x215,
                null, 100, 0);
            sh3_probs[sh3_probsMax++] = new SH3_Probs(0x209, 0x0);
            playSH3();
        }

        private void playSH3()
        {
            if (File.Exists(tbx_sh3path.Text) && Path.GetFileName(tbx_sh3path.Text) == "sh3.exe")
            {
                StartSH3Process();
            }
        }

        private void playSH2()
        {
            //if (File.Exists(txb_sh2_path.Text) && Path.GetFileName(txb_sh2_path.Text) == "sh2pc.exe")
            {
                StartSH2Process();
            }
        }

        //17: Handgun cinematic closer
        //240: Missionary
        //310: Leonard
        //397: Alessa
        //257 - 281: Town
        private static short[] doNotChangeSH3List = new short[] { 17, 240, 310, 397 };

        public void StartSH3Process()
        {
            tss_status.Text = "Loading game...";
            btn_play.Enabled = false;
            btn_lucky.Enabled = false;
            sh3_proc = Process.Start(tbx_sh3path.Text);
        }

        public void StartSH2Process()
        {
            tss_status.Text = "Please start sh2.pc.exe";
            btn_sh2_play.Enabled = false;
            btn_sh2_lucky.Enabled = false;
            lookforsh2 = true;
            //sh2_proc = Process.Start(txb_sh2_path.Text);
        }

        private bool SH3GidCanRando(short gid)
        {
            if (doNotChangeSH3List.Contains(gid)) return false;
            if (randoSHtown) return true;
            if (gid < 257 || gid > 281) return true;
            return false;
        }

        public void FuckWithSH3Process()
        {
            tss_status.Text = "Randomizing...";

            Console.WriteLine("Doing it...");

            {
                string s = "[";
                for (int i = 0; i != 100; i++)
                {
                    s += sh3_probs[i].id + ", " + sh3_probs[i].d + " | ";
                }
                s += "] "+ sh3_probsMax;
                Console.WriteLine(s);
            }

            Random r = new Random();
            Scribe.InitTo(sh3_proc);
            if (randomiseEnemies)
            {
                for (int i = 0; i != 40; i++)
                {
                    IntPtr ptr = new IntPtr(Scribe.ReadUInt32(new IntPtr(0x006cf7d0 + (i * 4))));
                    if (ptr != IntPtr.Zero)
                    {
                        IntPtr entsPtr = new IntPtr(Scribe.ReadUInt32(IntPtr.Add(ptr, 16)));
                        if (entsPtr != IntPtr.Zero)
                        {
                            while (true)
                            {
                                short typeID = Scribe.ReadInt16(entsPtr);
                                short gid = Scribe.ReadInt16(IntPtr.Add(entsPtr, 2));
                                if (typeID == 0) break;
                                if (SH3GidCanRando(gid) &&
                                    (typeID == 0x200 || typeID == 0x201 || typeID == 0x202 || typeID == 0x203 ||
                                    typeID == 0x204 || typeID == 0x205 || typeID == 0x206 || typeID == 0x20A ||
                                    typeID == 0x20B || typeID == 0x211 || typeID == 0x213 || typeID == 0x215))
                                {
                                    SH3_Probs p = sh3_probs[r.Next(0, sh3_probsMax)];
                                    Scribe.WriteInt16(entsPtr, p.id);
                                    Scribe.WriteInt16(IntPtr.Add(entsPtr, 22), p.d);
                                }
                                entsPtr = IntPtr.Add(entsPtr, 24);
                            }
                        }
                    }
                }
            }

            if (invisibleEnemies)
            {
                Scribe.WriteByte(new IntPtr(0x685fe3), 0x74);
                Scribe.WriteInt16(new IntPtr(0x685fdb), 0x0100);
                Scribe.WriteByte(new IntPtr(0x685ff1), 0x74);
            }

            Console.WriteLine("Done!");
            sh3_processHooked = true;
            tss_status.Text = "Playing";
        }

        private IntPtr[] sh2Regions = new IntPtr[]
        { IntPtr.Zero, new IntPtr(0x8F18A0), new IntPtr(0x8EAB50), new IntPtr(0x8EA718), new IntPtr(0x8EA208), new IntPtr(0x8EA208), new IntPtr(0x8E65D0), new IntPtr(0x8E5A58), new IntPtr(0x8E41A8), new IntPtr(0x8E3AC0),
            new IntPtr(0x8E30F8), new IntPtr(0x8E2708), new IntPtr(0x8E1F08), new IntPtr(0x8E1698), new IntPtr(0x8E08F0), new IntPtr(0x8DD888), new IntPtr(0x8DD380), new IntPtr(0x8DD238), new IntPtr(0x8DCD48), new IntPtr(0x8DC180),
            new IntPtr(0x8DB288), new IntPtr(0x8DA7D0), new IntPtr(0x8D9D68), new IntPtr(0x8D9798), new IntPtr(0x8D9218), new IntPtr(0x8D8CA8), new IntPtr(0x8D8130), new IntPtr(0x8D7870), new IntPtr(0x8D7620), new IntPtr(0x8D7318),
            new IntPtr(0x8D6ED8), new IntPtr(0x8D61E8), new IntPtr(0x8D5408), new IntPtr(0x8D4918), new IntPtr(0x8D4140), new IntPtr(0x8D2F00), new IntPtr(0x8D1FA8), new IntPtr(0x8CFCC8), new IntPtr(0x8CF618), new IntPtr(0x8CF310),
            new IntPtr(0x8CE9C8), new IntPtr(0x8CD0A0), new IntPtr(0x8CBE78), new IntPtr(0x8CB8F0), new IntPtr(0x8CAEC8), new IntPtr(0x8CA668), new IntPtr(0x8C9ED0), new IntPtr(0x8C9A58), new IntPtr(0x7B3D80), new IntPtr(0x7B4A58),
            new IntPtr(0x7B3058), new IntPtr(0x8C9610), new IntPtr(0x8C8C18), new IntPtr(0x8C8AB0), new IntPtr(0x8C8640), new IntPtr(0x8C7218), new IntPtr(0x8C6140), new IntPtr(0x8C5648), new IntPtr(0x8C5130) };

        public void FuckWithSH2Process()
        {
            tss_status.Text = "Randomizing...";

            Console.WriteLine("Doing it...");

            /*{
                string s = "[";
                for (int i = 0; i != 100; i++)
                {
                    s += sh3_probs[i].id + ", " + sh3_probs[i].d + " | ";
                }
                s += "] " + sh3_probsMax;
                Console.WriteLine(s);
            }*/

            Random r = new Random();
            Scribe.InitTo(sh2_proc);
            for (int i = 1; i != sh2Regions.Length; i++)
            {
                IntPtr entsPtr = new IntPtr(Scribe.ReadUInt32(IntPtr.Add(sh2Regions[i], 16)));
                if (entsPtr != IntPtr.Zero)
                {
                    while (true)
                    {
                        short first = Scribe.ReadInt16(entsPtr);
                        entsPtr = IntPtr.Add(entsPtr, 40);
                        if (first == 0) break;
                    }
                    while (true)
                    {
                        short typeID = Scribe.ReadInt16(entsPtr);
                        short gid = Scribe.ReadInt16(IntPtr.Add(entsPtr, 2));
                        if (typeID == 0) break;
                        if ((typeID == 0x200 || typeID == 0x201 || typeID == 0x202 || typeID == 0x207 ||
                            typeID == 0x208 || typeID == 0x20B))
                        {
                            SH2_Probs p = sh2_probs[r.Next(0, sh2_probsMax)];
                            Scribe.WriteInt16(entsPtr, p.id);
                        }
                        entsPtr = IntPtr.Add(entsPtr, 20);
                    }
                }
            }

            Console.WriteLine("Done!");
            sh2_processHooked = true;
            tss_status.Text = "Playing";
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 a = new AboutBox1();
            a.ShowDialog();
        }

        private void chb_randSHtown_CheckedChanged(object sender, EventArgs e)
        {
            randoSHtown = chb_randomSHtown.Checked;
        }

        private void chb_sh3invisEnemies_CheckedChanged(object sender, EventArgs e)
        {
            invisibleEnemies = chb_sh3invisEnemies.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            randomiseEnemies = chb_doEnemyRando.Checked;
        }

        private void norm_sh2_figure_TextChanged(object sender, EventArgs e)
        {
            OnMainSH2TextChanged(norm_sh2_figure, ref sh2_figurePercent);
        }

        private void norm_sh2_manekin_TextChanged(object sender, EventArgs e)
        {
            OnMainSH2TextChanged(norm_sh2_manekin, ref sh2_manekinPercent);
        }

        private void norm_sh2_creeper_TextChanged(object sender, EventArgs e)
        {
            OnMainSH2TextChanged(norm_sh2_creeper, ref sh2_bugPercent);
        }

        private void norm_sh2_nurse_TextChanged(object sender, EventArgs e)
        {
            OnMainSH2TextChanged(norm_sh2_nurse, ref sh2_nursePercent);
        }

        private void norm_sh2_darknurse_TextChanged(object sender, EventArgs e)
        {
            OnMainSH2TextChanged(norm_sh2_darknurse, ref sh2_darkNursePercent);
        }

        private void norm_sh2_ph_TextChanged(object sender, EventArgs e)
        {
            OnMainSH2TextChanged(norm_sh2_ph, ref sh2_phPercent);
        }

        private void sld_sh2_figure_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH2SliderChanged(sld_sh2_figure, ref sh2_figurePercent);
        }

        private void sld_sh2_manekin_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH2SliderChanged(sld_sh2_manekin, ref sh2_manekinPercent);
        }

        private void sld_sh2_creeper_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH2SliderChanged(sld_sh2_creeper, ref sh2_bugPercent);
        }

        private void sld_sh2_nurse_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH2SliderChanged(sld_sh2_nurse, ref sh2_nursePercent);
        }

        private void sld_sh2_darknurse_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH2SliderChanged(sld_sh2_darknurse, ref sh2_darkNursePercent);
        }

        private void sld_sh2_ph_ValueChanged(object sender, EventArgs e)
        {
            OnMainSH2SliderChanged(sld_sh2_ph, ref sh2_phPercent);
        }

        /*private void btn_sh2_lookforexe_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                txb_sh2_path.Text = openFileDialog2.FileName;
            }
        }*/

        private void btn_sh2_play_Click(object sender, EventArgs e)
        {
            playSH2();
        }

        private void btn_sh2_lucky_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            sh2_probsMax = 0;
            SetFinalSH2Probs(r.Next(1, 101), null, 0x200);
            SetFinalSH2Probs(r.Next(1, 101), null, 0x201);
            SetFinalSH2Probs(r.Next(1, 101), null, 0x202);
            SetFinalSH2Probs(r.Next(1, 101), null, 0x207);
            SetFinalSH2Probs(r.Next(1, 101), null, 0x208);
            SetFinalSH2Probs(r.Next(1, 101), null, 0x20B);
            playSH2();
        }

        private void txb_sh2_path_TextChanged(object sender, EventArgs e)
        {
            if (!sh2_processHooked)
            {
                /*if (File.Exists(txb_sh2_path.Text) && Path.GetFileName(txb_sh2_path.Text) == "sh2pc.exe")
                {
                    btn_sh2_play.Enabled = true;
                    btn_sh2_lucky.Enabled = true;
                    tss_status.Text = "Ready";
                }
                else
                {
                    btn_sh2_play.Enabled = false;
                    btn_sh2_lucky.Enabled = false;
                    tss_status.Text = "Need exe";
                }*/
            }
        }
    }
}
