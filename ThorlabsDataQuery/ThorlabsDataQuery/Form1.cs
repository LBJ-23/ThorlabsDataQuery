using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Thorlabs.TLBC1.Interop;
using Thorlabs.TLBC1.Interop.Structs;

namespace ThorlabsDataQuery
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 访问Thorlabs仪器的类
        /// </summary>
        private TLBC1 bc1Device = null;
        StringBuilder manufacturer = new StringBuilder(16);
        StringBuilder model = new StringBuilder(256);
        StringBuilder serialNumber = new StringBuilder(16);
        StringBuilder resourceName = new StringBuilder(256);


        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            dgvBasicData.Columns.Add("Name", "属性名");
            dgvBasicData.Columns.Add("value", "属性值");
            

            dgvCalData.Columns.Add("Name", "属性名");
            dgvCalData.Columns.Add("value", "属性值");

            if (ConnectToTheFirstDevice())
            {
                //bc1Device.get_effective_area(out double area);
                dgvBasicData.Rows.Add(new object[] { "manufacturer", manufacturer.ToString() });
                dgvBasicData.Rows.Add(new object[] { "model", model.ToString() });
                dgvBasicData.Rows.Add(new object[] { "serialNumber", serialNumber.ToString() });

                if(bc1Device.get_auto_calculation_area_clip_level(out double autoCalAreaClipLevel) == 0)
                {
                    dgvBasicData.Rows.Add(new object[] { "automatic calculation area clip level", autoCalAreaClipLevel.ToString("F3")});
                }

                if(bc1Device.get_averaging(out byte mode, out ushort value) == 0)
                {
                    dgvBasicData.Rows.Add(new object[] { "averaging mode", mode.ToString()});
                    dgvBasicData.Rows.Add(new object[] { "averaging value", value.ToString()});
                }

                if(bc1Device.get_calculation_area_mode(out bool automatic, out byte form) == 0)
                {
                    if (automatic)
                    {
                        dgvBasicData.Rows.Add(new object[] { "calculation mode", "Automatic" });
                    }                       
                    else
                    {
                        dgvBasicData.Rows.Add(new object[] { "calculation mode", "User defined" });
                    }
                     
                    if(form == 0)
                    {
                        dgvBasicData.Rows.Add(new object[] { "form of the calculation area", "Rectangle" });
                    }
                    else if(form == 1)
                    {
                        dgvBasicData.Rows.Add(new object[] { "form of the calculation area", "Ellipse" });
                    }
                }

                if(bc1Device.get_clip_level(out double clipLevel) == 0)
                {
                    dgvBasicData.Rows.Add(new object[] { "clip level", clipLevel.ToString("F3") });
                }

                if(bc1Device.get_measurement_method(out byte measurementMethod) == 0)
                {
                    if (measurementMethod == 0)
                    {
                        dgvBasicData.Rows.Add(new object[] { "measurement method", "所有像素" });
                    }
                    else if(measurementMethod == 1)
                    {
                        dgvBasicData.Rows.Add(new object[] { "measurement method", "狭缝仿真" });
                    }
                }

                if(bc1Device.get_wavelength(out double wavelength) == 0)
                {
                    dgvBasicData.Rows.Add(new object[] { "wavelength", wavelength.ToString("F3") });
                }
                  
                //bc1Device.get_effective_area(out double area);
                //bc1Device.get_ellipse_diameters(out double min, out double max, out double mean);
                if (bc1Device.get_scan_data(out TLBC1_calculations scanData) == 0)
                {
                    //var text = JsonConvert.SerializeObject((object)scanData);

                    //var  ojb = text.FromJsonString<TLBC1_calculations>();

                    dgvCalData.Rows.Add(new object[] { "EllipseEllipticity",  scanData.EllipseEllipticity});
                    dgvCalData.Rows.Add(new object[] { "max", scanData.EllipseDiaMax});
                    dgvCalData.Rows.Add(new object[] { "min", scanData.EllipseDiaMin });
                    dgvCalData.Rows.Add(new object[] { "CentroidPositionX", scanData.CentroidPositionX });
                    dgvCalData.Rows.Add(new object[] { "CentroidPositionY", scanData.CentroidPositionY });
                    dgvCalData.Rows.Add(new object[] { "EffectiveBeamDiameter", scanData.EffectiveBeamDiameter });
                    
                }
            }            
        }

        /// <summary>
        /// 搜索连接的设备并连接到第一个设备
        /// </summary>
        private bool ConnectToTheFirstDevice()
        {
            // 创建无效句柄，直到没有选择设备为止
            bc1Device = new TLBC1(new IntPtr(0xFF));

            // 获取已连接设备数量
            uint deviceCount;
            int res;

            res = bc1Device.get_device_count(out deviceCount);
            if (res == 0)
            {
                if (deviceCount > 0)
                {
                    
                    bool devAvailable;

                    // 获取第一个设备信息
                    res = bc1Device.get_device_information(0, manufacturer, model, serialNumber, out devAvailable, resourceName);

                    if (res == 0 && devAvailable)
                    {
                        //MeasurementSerialNumber = serialNumber.ToString();

                        // 连接到第一个可用设备
                        try
                        {
                            // 清除类的当前实例
                            bc1Device.Dispose();
                            bc1Device = null;

                            // 创建第一个设备连接实例
                            bc1Device = new TLBC1(resourceName.ToString(), false, false);

                            // 设置图像格式
                            res = bc1Device.set_precision_mode(0);

                            if (res == 0)
                            {
                                return true;
                            }
                            
                            res = bc1Device.set_wavelength(1060);

                            if (res == 0)
                            {
                                return true;
                            }
                        }
                        catch
                        {
                            bc1Device?.Dispose();
                            bc1Device = null;
                            return false;
                        }
                    }

                    bc1Device?.Dispose();
                    bc1Device = null;
                    return false;
                }
            }

            bc1Device?.Dispose();
            bc1Device = null;
            return false;

        }


        private void button1_Click(object sender, EventArgs e)
        {
            bc1Device.set_wavelength(1060);
            bc1Device.set_ellipse_mode(1);
            bc1Device.set_precision_mode(0);
            bc1Device.set_gain(1);
            bc1Device.set_auto_exposure(true);
            //bc1Device.set_exposure_time(0);
            bc1Device.set_max_hold(false);
            bc1Device.set_measurement_method(0);
            //bc1Device.set_averging(0, 2);
            bc1Device.set_auto_calculation_area_clip_level(0.01);
            bc1Device.set_clip_level(0.135);
            bc1Device.set_attenuation(-1);
            Getnew();
        }



        private void Getnew()
        {

            dgvBasicData.Rows.Clear();
            dgvCalData.Rows.Clear();

            dgvBasicData.Rows.Add(new object[] { "manufacturer", manufacturer.ToString() });
            dgvBasicData.Rows.Add(new object[] { "model", model.ToString() });
            dgvBasicData.Rows.Add(new object[] { "serialNumber", serialNumber.ToString() });

            if (bc1Device.get_auto_calculation_area_clip_level(out double autoCalAreaClipLevel) == 0)
            {
                dgvBasicData.Rows.Add(new object[] { "automatic calculation area clip level", autoCalAreaClipLevel.ToString("F3") });
            }

            if (bc1Device.get_averaging(out byte mode, out ushort value) == 0)
            {
                dgvBasicData.Rows.Add(new object[] { "averaging mode", mode.ToString() });
                dgvBasicData.Rows.Add(new object[] { "averaging value", value.ToString() });
            }

            if (bc1Device.get_calculation_area_mode(out bool automatic, out byte form) == 0)
            {
                if (automatic)
                {
                    dgvBasicData.Rows.Add(new object[] { "calculation mode", "Automatic" });
                }
                else
                {
                    dgvBasicData.Rows.Add(new object[] { "calculation mode", "User defined" });
                }

                if (form == 0)
                {
                    dgvBasicData.Rows.Add(new object[] { "form of the calculation area", "Rectangle" });
                }
                else if (form == 1)
                {
                    dgvBasicData.Rows.Add(new object[] { "form of the calculation area", "Ellipse" });
                }
            }

            if (bc1Device.get_clip_level(out double clipLevel) == 0)
            {
                dgvBasicData.Rows.Add(new object[] { "clip level", clipLevel.ToString("F3") });
            }

            if (bc1Device.get_measurement_method(out byte measurementMethod) == 0)
            {
                if (measurementMethod == 0)
                {
                    dgvBasicData.Rows.Add(new object[] { "measurement method", "所有像素" });
                }
                else if (measurementMethod == 1)
                {
                    dgvBasicData.Rows.Add(new object[] { "measurement method", "狭缝仿真" });
                }
            }

            if (bc1Device.get_wavelength(out double wavelength) == 0)
            {
                dgvBasicData.Rows.Add(new object[] { "wavelength", wavelength.ToString("F3") });
            }

            //bc1Device.get_effective_area(out double area);
            //bc1Device.get_ellipse_diameters(out double min, out double max, out double mean);
            if (bc1Device.get_scan_data(out TLBC1_calculations scanData) == 0)
            {
                var text = JsonConvert.SerializeObject((object)scanData);


                //var  ojb = text.FromJsonString<TLBC1_calculations>();

                dgvCalData.Rows.Add(new object[] { "EllipseEllipticity", scanData.EllipseEllipticity });
                dgvCalData.Rows.Add(new object[] { "max", scanData.EllipseDiaMax });
                dgvCalData.Rows.Add(new object[] { "min", scanData.EllipseDiaMin });
                dgvCalData.Rows.Add(new object[] { "CentroidPositionX", scanData.CentroidPositionX });
                dgvCalData.Rows.Add(new object[] { "CentroidPositionY", scanData.CentroidPositionY });
                dgvCalData.Rows.Add(new object[] { "EffectiveBeamDiameter", scanData.EffectiveBeamDiameter });

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string text = "";
            if (bc1Device.get_scan_data(out TLBC1_calculations scanData) == 0)
            {
                text = JsonConvert.SerializeObject((object)scanData);
            }

            bc1Device.get_wavelength(out double wavelength);
            bc1Device.get_clip_level(out double clipLevel);

            //bc1Device.get_effective_area(out double area);
            //bc1Device.get_ellipse_diameters(out double min, out double max, out double mean);


            DirectoryInfo directoryInfo = new DirectoryInfo("D:\\YYC\\project\\ThorlabsDll");        


            FileInfo file = new FileInfo("D:\\YYC\\project\\ThorlabsDll\\Thorlabs_11_28.txt");

            using (StreamWriter sw = new StreamWriter(file.FullName, true, Encoding.Default))
            {
                sw.WriteLine(DateTime.Now.ToString());
                sw.WriteLine(text);
                sw.WriteLine($"wavelength:{wavelength}");
                sw.WriteLine($"clipLevel:{clipLevel}");
                //sw.WriteLine($"effective_area:{area}");
                //sw.WriteLine($"ellipse_diameters: min:{min} max:{max} mean:{mean}");
            }
        }
    }
}
