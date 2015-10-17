using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Tools;
using System.Threading;

namespace WinFormsClient.WBMode
{
    public class WBTaoDurexValidationMode : WebBrowserMode<WBTaoDurexValidationMode>
    {
        public async Task Start(string durexParam)
        {
            string validateurl = "http://aq.taobao.com/durex/validate?param=" + durexParam + "&redirecturl=http%3A%2F%2Fchongzhi.taobao.com%2Ferror.do%3Fmethod%3DdurexJump";
            var validateSucessUrl = "http://chongzhi.taobao.com/error.do?method=durexJump&type=success";
            await this.SynchronousLoadDocument(validateurl, validateSucessUrl, 60, CancellationToken.None);
        }
        protected override void EnterModeInternal(ExtendedWinFormsWebBrowser webBrowser)
        {

        }

        protected override void LeaveModeInternal(ExtendedWinFormsWebBrowser webBrowser)
        {

        }
    }
}
