using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace iTextPdfTest
{
    public class iTextSharpFunction
    {
        [FunctionName("TestPdfOutput")]
        public HttpResponseMessage TestPdfOutput(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "test_output_pdf")] HttpRequest req,
            ILogger log
            )
        {
            // 日本語を使う場合に下記三行が必要:
            // エラー解消：'windows-1252' is not a supported encoding name.
            System.Text.EncodingProvider encodingProvider;
            encodingProvider = System.Text.CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encodingProvider);

            // A4横でドキュメントを作成
            var doc = new Document(PageSize.A4.Rotate());
            var ms = new MemoryStream();

            //ファイルの出力先を設定
            var pw = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, ms);

            //ドキュメントを開く
            doc.Open();

            var pdfContentByte = pw.DirectContent;
            //var bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.WINANSI, BaseFont.EMBEDDED);
            var bf = BaseFont.CreateFont(@"MS Gothic.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);

            // ベースフォントとフォントサイズを指定する。
            pdfContentByte.SetFontAndSize(bf, 10);

            // 指定箇所に文字列を記述
            ShowTextAligned(pdfContentByte, 10, 500, "Test Pdf Output");
            ShowTextAligned(pdfContentByte, 10, 480, "日本語の出力テスト。");
            DrawLine(pdfContentByte, 2, 451, 840, 451);

            doc.Close();

            /*-- ファイル出力
            using (BinaryWriter w = new BinaryWriter(File.OpenWrite(@"result.pdf")))
            {
                w.Write(ms.ToArray());
            }
            --*/

            // レスポンス作成
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(ms.GetBuffer())
            };

            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "testhoge.pdf"
            };

            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

            return response;
        }

        [FunctionName("TestPdfOutputOverwrite")]
        public HttpResponseMessage TestPdfOutputOverwrite(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "test_output_pdf")] HttpRequest req,
            ILogger log
        )
        {
            EncodingProvider encodingProvider;
            encodingProvider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encodingProvider);

            // 取得したPDFのサイズでDocument作成
            var reader = new PdfReader(@"Google.pdf");
            var doc = new Document(reader.GetPageSize(1));
            var ms = new MemoryStream();

            //ファイルの出力先を設定
            var pw = PdfWriter.GetInstance(doc, ms);

            //ドキュメントを開く
            doc.Open();

            var pdfContentByte = pw.DirectContent;

            // テンプレートのページを追加する
            doc.NewPage();
            var page = pw.GetImportedPage(reader, 1);
            pdfContentByte.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);

            var bf = BaseFont.CreateFont(@"MS Gothic.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);

            // ベースフォントとフォントサイズを指定する。
            pdfContentByte.SetFontAndSize(bf, 10);

            // 指定箇所に文字列を記述
            ShowTextAligned(pdfContentByte, 10, 500, "Googleのページやで！");
            ShowTextAligned(pdfContentByte, 10, 480, "日本語の出力テスト。");
            DrawLine(pdfContentByte, 2, 451, 840, 451);

            doc.Close();

            // レスポンス作成
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(ms.GetBuffer())
            };

            //Content-Dispositionをattachmentでファイル名を指定していたらプレビューが開かない
            //response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            //{
            //    FileName = "testhoge.pdf"
            //};

            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

            return response;

        }
        private void ShowTextAligned(PdfContentByte pdfContentByte, float x, float y, string text, int alignment = Element.ALIGN_LEFT, float rotaion = 0)
        {
            pdfContentByte.BeginText();
            pdfContentByte.ShowTextAligned(alignment, text, x, y, rotaion);
            pdfContentByte.EndText();
        }

        private static void DrawLine(PdfContentByte pdfContentByte, float fromX, float fromY, float toX, float toY)
        {
            pdfContentByte.MoveTo(fromX, fromY);
            pdfContentByte.LineTo(toX, toY);
            pdfContentByte.ClosePathStroke();
        }
    }
}