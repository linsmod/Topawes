using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public static class WebBrowserExtension
    {
        private static void createJSElementForJSONPHandler(this WebBrowser wb)
        {
            var setupJSONPHandler = @"
                function handleJSONP(d){
                    var cnt=document.getElementById('jsonpcontent');
                    if(cnt==null){
                        cnt=document.createElement('div');
                        cnt.setAttribute('id','jsonpcontent');
                        document.body.appendChild(cnt);
                    }
                    cnt.innerText=JSON.stringify(d);
                };
                function getJSONPContent(){
                    var cnt=document.getElementById('jsonpcontent');
                    var text= cnt==null?'':cnt.innerText;
                    return text;
                };
";
            wb.AppendJsElement("jsonpHandler", setupJSONPHandler);
        }

        /// <summary>
        /// url?callback=handleJSONP, after wb completed event, #jsonpcontent will contains response
        /// </summary>
        /// <param name="wb"></param>
        /// <param name="url"></param>
        public static async Task<string> ExecuteTriggerJSONP(this WebBrowser wb, string url)
        {
            url += "&callback=handleJSONP";
            wb.createJSElementForJSONPHandler();
            var setupJSONPTrigger = @"
                function triggerJSONP(url){
                    var _name = 'jsonptrigger';
                    var js = document.createElement('script');
                    js.setAttribute('id',_name);
                    //FF:onload, IE:onreadystatechange
                    js.onload = js.onreadystatechange = function(){
                        if (!this.readyState || this.readyState === 'loaded' || this.readyState === 'complete')
                        {
                            js.onload = js.onreadystatechange = null//请内存，防止IE memory leaks
                            js.remove();
                        }
                    }
                    js.type = 'text/javascript';
                    js.src = url;
                    document.getElementsByTagName('head')[0].appendChild(js);
                    var c
                }";
            wb.AppendJsElement("jsonpTrigger", setupJSONPTrigger);
            wb.Document.InvokeScript("triggerJSONP", new object[] { url });
            await TaskEx.Delay(200);
            object content = null;
            while ((content = wb.Document.InvokeScript("getJSONPContent")) == null || content.ToString() == "")
            {
                await TaskEx.Delay(200);
                if (wb.IsBusy)
                    continue;
            }
            
            return content.ToString();
        }

        private static void createXHR(this WebBrowser wb)
        {
            var createXHR = @"function createXHR(){
                                if(typeof XMLHttpRequest!='undefined'){
                                    return new HMLHttpRequest();
                                }
                                else if(typeof ActiveXObject!='undefined'){
                                     if(typeof arguments.callee.activeXString!='string'){
                                        var versions = ['MSXML2.XMLHTTP.6.0','MSXML2.XMLHTTP.3.0','MSXML2.XMLHTTP'],i,len;
                                        for(i=0,len=versions.length;i<len;i++){
                                            try{
                                                new ActiveXObject(versions[i]);
                                                arguments.callee.activeXString = versions[i];
                                                break;
                                            } catch(e){}
                                        }
                                     }
                                     return new ActiveXObject(arguments.callee.activeXString);
                                }
                                else{ throw new Error('No XHR object available.');}
                            };";
            var JSElement = wb.Document.GetElementById("createXHR");
            if (JSElement == null)
            {
                JSElement = wb.Document.CreateElement("script");
                JSElement.SetAttribute("type", "text/javascript");
                JSElement.SetAttribute("id", "createXHR");
                JSElement.SetAttribute("text", createXHR);
                HtmlElement head = wb.Document.Body.AppendChild(JSElement);
            }
        }

        private static void createXHRDataElement(this WebBrowser wb)
        {
            HtmlElement xhrcontent = wb.Document.GetElementById("xhrcontent");
            if (xhrcontent == null)
            {
                xhrcontent = wb.Document.CreateElement("div");
                xhrcontent.SetAttribute("id", "xhrcontent");
                wb.Document.Body.AppendChild(xhrcontent);
            }
        }

        public static void XHRGet(this WebBrowser wb, string url)
        {
            wb.createXHR();
            wb.createXHRDataElement();
            var xhrGet = @"  function xhrGet(url){
                                var xhr = createXHR();
                                xhr.onreadystatechange = function(){
                                    if((xhr.status>=200&&xhr.stats<300)||xhr.status==304){document.getElementById('xhrcontent').html(xhr.responseText);}
                                    else{document.getElementById('xhrcontent').innerHtml='xhr.status='+xhr.status;}
                                }
                                xhr.open('get',url,true);
                                xhr.send(null);
                            }";
            var JSElement = wb.Document.GetElementById("xhrGet");
            if (JSElement == null)
            {
                JSElement = wb.Document.CreateElement("script");
                JSElement.SetAttribute("type", "text/javascript");
                JSElement.SetAttribute("id", "xhrGet");
                JSElement.SetAttribute("text", xhrGet);
                HtmlElement head = wb.Document.Body.AppendChild(JSElement);
                JSElement.SetAttribute("src", "http://baidu.com/");
            }
            wb.Document.InvokeScript("xhrGet", new string[] { url });
        }

        private static void AppendJsElement(this WebBrowser wb, string id, string content)
        {
            HtmlElement JSElement = wb.Document.GetElementById(id);
            if (JSElement == null)
            {
                JSElement = wb.Document.CreateElement("script");
                JSElement.SetAttribute("type", "text/javascript");
                JSElement.SetAttribute("id", id);
                JSElement.SetAttribute("text", content);
                wb.Document.Body.AppendChild(JSElement);
            }
        }
    }
}
