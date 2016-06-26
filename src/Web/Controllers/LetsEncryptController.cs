using Microsoft.AspNetCore.Mvc;

public class LetsEncryptController : Controller
    {
        [HttpGet]
        [Route(".well-known/acme-challenge/5Q_q4R2FUTPnQIoQ45hbfbvl3qz4BCqw02V0oeRAOiw")]
        public string Verify()
        {
            Response.ContentType="text/plain";
            return "5Q_q4R2FUTPnQIoQ45hbfbvl3qz4BCqw02V0oeRAOiw.bJFo0_9avJjgL6RNeBxyurjDMHI8HdakeP7Eho5wo7E";
        }
    }