using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

    public class ImageController : MonoBehaviour
    {
        public RawImage rawImage;
        int currentImg;
        List<Texture2D> textures = new List<Texture2D>();

        float timer, timerLength = 30;
        bool startTimer;

        private void Awake()
        {
            LoadImage();


        }

        void LoadImage()
        {
            //For Choosing a file
            string folderPath;
            string[] filePaths;
            int i = 0;

            folderPath = Application.streamingAssetsPath;
            filePaths = Directory.GetFiles(folderPath, "*.png");
            foreach (string filepath in filePaths)
            {
                if (filepath.Contains(".meta"))
                    continue;
                Texture2D texture2D = new Texture2D(2, 2);

                try
                {
                    byte[] pngBytes = System.IO.File.ReadAllBytes(filepath);
                    texture2D.LoadImage(pngBytes);
                }
                catch (System.Exception)
                {

                    throw;
                }
                textures.Add(texture2D);
                // rawImage.GetComponent<RawImage>().texture = texture2D;
                i++;
            }
            Debug.Log(i);


        }
        public void LoadImage(int dir)
        {

            if (dir > 0)
            {
                if (currentImg + dir <= textures.Count - 1)
                {
                    currentImg += dir;
                }
                else
                    currentImg = 0;
            }
            else if (dir < 0)
            {
                if (currentImg + dir >= 0)
                    currentImg += dir;
                else
                    currentImg = textures.Count - 1;

            }

            rawImage.texture = textures[currentImg];

        }
    }

