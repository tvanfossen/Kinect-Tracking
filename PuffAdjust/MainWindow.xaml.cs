using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Windows;
using Microsoft.Kinect;
using LightBuzz.Vitruvius;


namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        PlayersController _playersController;
        GestureController _gestureController;


        public bool IsTracked { get; }

        double height;
        int heightFlag = 0;
        int noHeightFlag = 0;
        int heightcm;
        int Adder;

        String SendString;
        String GestureString;

        public MainWindow()
        {
            InitializeComponent();
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

                _gestureController = new GestureController();
                _gestureController.GestureRecognized += GestureController_GestureRecognized;


                _playersController = new PlayersController();
                _playersController.BodyEntered += UserReporter_BodyEntered;
                _playersController.BodyLeft += UserReporter_BodyLeft;
                _playersController.Start();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_playersController != null)
            {
                _playersController.Stop();
            }

            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }
       
        public void SendData()
        {
            SerialPort port = new SerialPort("COM4", 9600);
            port.Open();
            if (heightFlag == 0)
            {
                heightFlag = 1;
                noHeightFlag = 1;

                port.Write(SendString);
                
            }

            port.Close();
            

        }
      


        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (viewer.Visualization == Visualization.Color)
                    {
                        viewer.Image = frame.ToBitmap();
                    }
                }
            }



            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var bodies = frame.Bodies();
                    _playersController.Update(bodies);


                    Body bodyg = frame.Bodies().Closest();

                    viewer.DrawBody(bodyg);

                    if (bodyg != null)
                    {
                        _gestureController.Update(bodyg);
                        if (heightFlag == 0)
                        {
                            height = 100 * (bodyg.Height());
                           
                            textBox.Text = height.ToString();
                            heightcm = (Convert.ToInt32(height)) + Adder;
                            SendString = "X" + heightcm.ToString() + '\n';

                            noHeightFlag = 0;

                            SendData();
                        }
                    }
                    else if (bodyg== null)
                    {
                        heightFlag = 0;
 
                    }
                    if (GestureString == "ZoomIn")
                    {
                        heightFlag = 0;
                        GestureString = null;
                        textBox2.Text = null;

                        SendString = "X" + heightcm.ToString() + '\n';
                    }
                   /*if (GestureString == "SwipeLeft")
                    {
                        heightFlag = 0;
                        GestureString = null;

                        SendString = "A"+ '\n';

                        SendData();
                    }
                    else if (GestureString == "SwipeRight")
                    {
                        heightFlag = 0;
                        GestureString = null;

                        SendString = "E" + '\n';

                        SendData();
                    }*/
                }
            }
        }
        void GestureController_GestureRecognized(object sender, GestureEventArgs e)
        {
            textBox2.Text = e.GestureType.ToString();
            GestureString = e.GestureType.ToString();

            
        }

        void UserReporter_BodyEntered(object sender, PlayersControllerEventArgs e)
        {
            // A new user has entered the scene.
        }

        void UserReporter_BodyLeft(object sender, PlayersControllerEventArgs e)
        {
            // A user has left the scene.
            viewer.Clear();
        }
    }
}