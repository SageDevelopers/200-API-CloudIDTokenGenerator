using System;
using System.Windows.Forms;

namespace SageCloudIDTokenGeneratorv1._1
{
    public partial class CloudIDTokenGen : Form
    {
        public CloudIDTokenGen()
        {
            InitializeComponent();
        }

        private void buttonGenerateAccessToken_Click(object sender, EventArgs e)
        {
            string token = AuthenticationProviderFactory.GetProvider().GetToken();
            Clipboard.SetText("Bearer " + token);
            MessageBox.Show("Access Token Copied to Clipboard");
        }
    }
}
