using UnityEngine;
using UnityEngine.UI;
using EasyChart.UGUI;
using EasyChart.UIToolKit;
using System.Collections.Generic;

namespace EasyChart.Samples
{
    /// <summary>
    /// Simple example demonstrating how to use UGUIRuntimeJsonInjection or UIToolKitRuntimeJsonInjection
    /// to update chart data via code. Attach this script to a GameObject with a Button.
    /// </summary>
    public class JsonInjectionExample : MonoBehaviour
    {
        [Header("References (assign one)")]
        [Tooltip("Reference to the UGUIRuntimeJsonInjection component on the chart (for UGUI)")]
        [SerializeField] private UGUIRuntimeJsonInjection _uguiJsonInjection;

        [Tooltip("Reference to the UIToolKitRuntimeJsonInjection component on the chart (for UI Toolkit).\nPlace this component on the same GameObject as UIDocument.")]
        [SerializeField] private UIToolKitRuntimeJsonInjection _uiToolkitJsonInjection;

        [Tooltip("Optional: Specify which ChartElement to update by name.\nFor UIToolKit with multiple charts, set the chart element name here.\nIf empty, updates the first ChartElement found.")]
        [SerializeField] private string _chartElementName = "";

        [Tooltip("Optional: Button to trigger the update. If not set, will try to get from this GameObject.")]
        [SerializeField] private Button _updateButton;

        [Header("Sample JSON Data")]
        [Tooltip("The JSON data to inject into the chart when button is clicked")]
        [TextArea(10, 20)]
        public string JsonData = @"{
  ""series"": [
    {
      ""name"": ""Sales"",
      ""data"": [
        { ""x"": 0, ""value"": 120 },
        { ""x"": 1, ""value"": 200 },
        { ""x"": 2, ""value"": 150 },
        { ""x"": 3, ""value"": 80 },
        { ""x"": 4, ""value"": 170 }
      ]
    }
  ]
}";

        private List<int> _speeds = new List<int>();

        private void Start()
        {
            // Auto-get button if not assigned
            if (_updateButton == null)
            {
                _updateButton = GetComponent<Button>();
            }

            // Register button click listener
            if (_updateButton != null)
            {
                _updateButton.onClick.AddListener(OnUpdateButtonClicked);
            }
            else
            {
                Debug.LogWarning("[JsonInjectionExample] No Button found. Call UpdateChart() manually.");
            }
        }

        private void OnDestroy()
        {
            if (_updateButton != null)
            {
                _updateButton.onClick.RemoveListener(OnUpdateButtonClicked);
            }
        }

        /// <summary>
        /// Called when the update button is clicked.
        /// </summary>
        private void OnUpdateButtonClicked()
        {
            UpdateChart();
        }

        /// <summary>
        /// Update the chart with the current JsonData.
        /// Can be called from code or UnityEvent.
        /// </summary>
        /// 

        public void UpdateSpeed(int speed)
        {
            string randomJson = GenerateSpeedJson(speed);
            JsonData = randomJson;
            UpdateChart();
        }


        public void UpdateChart()
        {
            _uiToolkitJsonInjection.JsonContent = JsonData;
            _uiToolkitJsonInjection.ApplyJsonToChart();
        }

        /// <summary>
        /// Example: Update chart with random data (can be called from button or code).
        /// </summary>
        public void UpdateChartWithRandomData()
        {
            // Generate random data
            string randomJson = GenerateRandomJson();
            JsonData = randomJson;
            UpdateChart();
        }

        private string GenerateSpeedJson(int speed)
        {
            _speeds.Add(speed);
            if (_speeds.Count > 20)
            {
                _speeds.RemoveAt(0);
            }
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"code\": 200,");
            sb.AppendLine("  \"message\": \"success\",");
            sb.AppendLine("  \"data\": {");
            sb.AppendLine("    \"chartName\": \"Line\",");
            sb.AppendLine("    \"series\": [");
            sb.AppendLine("      {");
            sb.AppendLine("        \"name\": \"Series 1\",");
            sb.AppendLine("        \"datas\": [");

            int pointCount = _speeds.Count;
            for (int i = 0; i < pointCount; i++)
            {
                int value = _speeds[i];
                string comma = i < pointCount - 1 ? "," : "";
                sb.AppendLine($"          {{ \"x\": {i}, \"value\": {value} }}{comma}");
            }

            sb.AppendLine("        ]");
            sb.AppendLine("      }");
            sb.AppendLine("    ]");
            sb.AppendLine("  }");
            sb.AppendLine("}");

            return sb.ToString();
        }


        private string GenerateRandomJson()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"series\": [");
            sb.AppendLine("    {");
            sb.AppendLine("      \"name\": \"Line\",");
            sb.AppendLine("      \"data\": [");

            int pointCount = 8;
            for (int i = 0; i < pointCount; i++)
            {
                int value = Random.Range(50, 200);
                string comma = i < pointCount - 1 ? "," : "";
                sb.AppendLine($"        {{ \"x\": {i}, \"value\": {value} }}{comma}");
            }

            sb.AppendLine("      ]");
            sb.AppendLine("    }");
            sb.AppendLine("  ]");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
