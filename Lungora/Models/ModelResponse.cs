﻿namespace Lungora.Models
{
    public class ModelResponse
    {
        public bool is_success { get; set; }
        public bool is_upload { get; set; }
        public bool check_status { get; set; }
        public string predicted_label { get; set; }
    }

}
