using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace KotBot.Services
{
    public class ThermalPrinter
    {
        private readonly string _printerIp;
        private readonly int _printerPort;

        public ThermalPrinter(string printerIp, int printerPort = 9100)
        {
            _printerIp = printerIp;
            _printerPort = printerPort;
        }

        public void PrintReceipt(Receipt receipt)
        {
            using (var client = new TcpClient())
            {
                try
                {
                    client.Connect(_printerIp, _printerPort);
                    using (var stream = client.GetStream())
                    {
                        var sb = new StringBuilder();

                        // ESC/POS Commands
                        byte[] boldOn = new byte[] { 0x1B, 0x21, 0x20 }; // Bold on
                        byte[] boldOff = new byte[] { 0x1B, 0x21, 0x00 }; // Bold off
                        byte[] quadrupleHeightWidth = new byte[] { 0x1D, 0x21, 0x42 }; // Quadruple height and width (largest font)
                        byte[] normalSize = new byte[] { 0x1D, 0x21, 0x00 }; // Normal size
                        byte[] alignCenter = new byte[] { 0x1B, 0x61, 0x01 }; // Center align
                        byte[] alignLeft = new byte[] { 0x1B, 0x61, 0x00 }; // Left align
                        byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x00 }; // Cut paper

                        // Clear StringBuilder before use
                        sb.Clear();

                        // Center, Bold, and Quadruple Height/Width for Restaurant Name
                        stream.Write(alignCenter, 0, alignCenter.Length); // Center align
                        stream.Write(quadrupleHeightWidth, 0, quadrupleHeightWidth.Length); // Quadruple height and width
                        stream.Write(boldOn, 0, boldOn.Length); // Bold on

                        // If the restaurant name is too long, split it into multiple lines
                        if (receipt.RestaurantName.Length > 20) // Assuming 20 characters fit in one line
                        {
                            sb.AppendLine(receipt.RestaurantName.Substring(0, 20)); // First part
                            sb.AppendLine(receipt.RestaurantName.Substring(20)); // Remaining part
                        }
                        else
                        {
                            sb.AppendLine(receipt.RestaurantName); // Restaurant name from receipt
                        }

                        var restaurantBytes = Encoding.ASCII.GetBytes(sb.ToString()); // Convert to bytes
                        stream.Write(restaurantBytes, 0, restaurantBytes.Length); // Write to stream
                        stream.Write(boldOff, 0, boldOff.Length); // Bold off
                        stream.Write(normalSize, 0, normalSize.Length); // Back to normal size
                        sb.Clear(); // Clear StringBuilder after name

                        // Add extra spacing after restaurant name to avoid crowding
                        sb.AppendLine();
                        sb.AppendLine("-------------------------------------------");

                        // Left align for items
                        stream.Write(alignLeft, 0, alignLeft.Length);

                        // Medium Bold for headers (Item, Qty, Price)
                        stream.Write(boldOn, 0, boldOn.Length); // Bold on for headers
                        sb.AppendLine("Item                 Qty           Price");
                        stream.Write(boldOff, 0, boldOff.Length); // Bold off after headers

                        // Items
                        foreach (var item in receipt.Items)
                        {
                            sb.AppendLine($"{item.Name,-20} {item.Quantity,3}           {item.Price,8:F2}");
                        }

                        sb.AppendLine("-------------------------------------------");

                        // Medium Bold for Totals (Subtotal, Tax, Total Amount)
                        stream.Write(boldOn, 0, boldOn.Length); // Start bold for totals
                        sb.AppendLine($"Subtotal:                        {receipt.Subtotal,10:F2}");
                        sb.AppendLine($"Tax:                             {receipt.Tax,10:F2}");
                        sb.AppendLine($"Total Amount:                    {receipt.TotalAmount,10:F2}");
                        stream.Write(boldOff, 0, boldOff.Length); // End bold for totals
                        sb.AppendLine("-------------------------------------------");

                        // Add the "Thank You" message before cutting the paper
                        stream.Write(alignCenter, 0, alignCenter.Length);
                        sb.AppendLine("Thank You  !!!");
                        sb.AppendLine("Visit Again  !!!");

                        // Add a few new lines to ensure proper space after the message
                        sb.AppendLine(new string('\n', 3));

                        // Write the message to the printer
                        var bytes = Encoding.ASCII.GetBytes(sb.ToString());
                        stream.Write(bytes, 0, bytes.Length);

                        // Flush the stream to ensure all data is sent to the printer
                        stream.Flush();

                        // Cut the paper after the "Thank You" message
                        stream.Write(cutPaper, 0, cutPaper.Length);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error printing receipt: {ex.Message}");
                }
            }
        }
    }

    public class Receipt
    {
        public string RestaurantName { get; set; } // Dynamic restaurant name
        public List<ReceiptItem> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }

        public Receipt()
        {
            Items = new List<ReceiptItem>();
        }
    }

    public class ReceiptItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
