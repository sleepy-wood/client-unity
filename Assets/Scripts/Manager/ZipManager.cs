using System.IO;

using System.Collections;

using ICSharpCode.SharpZipLib.Zip;



public class ZipManager
{
    public static float ProgressValue = 0;
    public static bool retVal = false;

    /// 

    /// 특정 폴더를 ZIP으로 압축 

    /// 

    /// 압축 대상 폴더 경로 
    /// 저장할 ZIP 파일 경로 
    /// 압축 암호 
    /// 폴더 삭제 여부 
    /// 압축 성공 여부 

    public static bool ZipFiles(string targetFolderPath, string zipFilePath, bool isDeleteFolder)//,string password, )
    {
        // 폴더가 존재하지 않는 경우 만들기
        if (!Directory.Exists(targetFolderPath))
        {
            Directory.CreateDirectory(targetFolderPath);
        }
        // 압축 대상 폴더의 파일 목록. 
        ArrayList ar = GenerateFileList(targetFolderPath);

        // 압축 대상 폴더 경로의 길이 + 1 
        int TrimLength =
               (Directory.GetParent(targetFolderPath)).ToString().Length + 1;
        // find number of chars to remove. from orginal file path. remove '\' 

        FileStream ostream;
        byte[] obuffer;
        string outPath = zipFilePath;

        // ZIP 스트림 생성. 
        ZipOutputStream oZipStream = new ZipOutputStream(File.Create(outPath));
        try
        {
            // 패스워드가 있는 경우 패스워드 지정. 
            //if (password != null && password != string.Empty)
            //    oZipStream.Password = password;

            //oZipStream.SetLevel(9); // 암호화 레벨.(최대 압축) 

            ZipEntry oZipEntry;
            foreach (string Fil in ar)
            {
                oZipEntry = new ZipEntry(Fil.Remove(0, TrimLength));
                oZipStream.PutNextEntry(oZipEntry);

                // 파일인 경우. 
                if (!Fil.EndsWith(@"/"))
                {
                    ostream = File.OpenRead(Fil);
                    obuffer = new byte[ostream.Length];
                    ostream.Read(obuffer, 0, obuffer.Length);
                    oZipStream.Write(obuffer, 0, obuffer.Length);
                }
            }

            retVal = true;
        }
        catch
        {
            retVal = false;
            // 오류가 난 경우 생성 했던 파일을 삭제. 
            if (File.Exists(outPath))
                File.Delete(outPath);
        }
        finally
        {
            // 압축 종료. 
            oZipStream.Finish();
            oZipStream.Close();
        }


        // 폴더 삭제를 원할 경우 폴더 삭제. 
        if (isDeleteFolder)
            try
            {
                Directory.Delete(targetFolderPath, true);
            }
            catch { }

        return retVal;
    }

    /// 

    /// 파일, 폴더 목록 생성 

    /// 

    /// 폴더 경로 
    /// 폴더, 파일 목록(ArrayList) 

    private static ArrayList GenerateFileList(string Dir)
    {
        ArrayList fils = new ArrayList();
        bool Empty = true;
        // 폴더 내의 파일 추가. 
        foreach (string file in Directory.GetFiles(Dir))
        {
            fils.Add(file);
            Empty = false;
        }
        if (Empty)
        {
            // 파일이 없고, 폴더도 없는 경우 자신의 폴더 추가. 
            if (Directory.GetDirectories(Dir).Length == 0)
                fils.Add(Dir + @"/");
        }
        // 폴더 내 폴더 목록. 
        foreach (string dirs in Directory.GetDirectories(Dir))
        {
            // 해당 폴더로 다시 GenerateFileList 재귀 호출 
            foreach (object obj in GenerateFileList(dirs))
            {
                // 해당 폴더 내의 파일, 폴더 추가. 
                fils.Add(obj);
            }
        }
        return fils;
    }

    /// 
    /// 압축 파일 풀기 
    /// 
    /// ZIP파일 경로 
    /// 압축 풀 폴더 경로 
    /// 해지 암호 
    /// zip파일 삭제 여부 
    /// 압축 풀기 성공 여부  

    //public static IEnumerator UnZipFiles(string zipFilePath, string unZipTargetFolderPath,
    //                                string password, bool isDeleteZipFile)
    //{

    //    // ZIP 파일이 있는 경우만 수행. 
    //    if (File.Exists(zipFilePath))
    //    {
    //        // ZIP 스트림 생성. 
    //        ZipInputStream zipInputStream = new ZipInputStream(File.OpenRead(zipFilePath));


    //        // 패스워드가 있는 경우 패스워드 지정. 
    //        if (password != null && password != string.Empty)
    //            zipInputStream.Password = password;

    //        try
    //        {
    //            ZipEntry theEntry;
    //            long Count = 0;
    //            // 반복하며 파일을 가져옴. 
    //            while ((theEntry = zipInputStream.GetNextEntry()) != null)
    //            {
    //                // 폴더 
    //                string directoryName = Path.GetDirectoryName(theEntry.Name);
    //                string fileName = Path.GetFileName(theEntry.Name); // 파일 

    //                // 폴더 생성 
    //                Directory.CreateDirectory(unZipTargetFolderPath + directoryName);

    //                // 파일 이름이 있는 경우 
    //                if (fileName != string.Empty)
    //                {
    //                    // 파일 스트림 생성.(파일생성) 
    //                    FileStream streamWriter =
    //                          File.Create((unZipTargetFolderPath + theEntry.Name));

    //                    int size = 2048;
    //                    byte[] data = new byte[2048];

    //                    // 파일 복사 
    //                    while (true)
    //                    {
    //                        size = zipInputStream.Read(data, 0, data.Length);

    //                        if (size > 0)
    //                            streamWriter.Write(data, 0, size);
    //                        else
    //                            break;
    //                    }

    //                    // 파일스트림 종료 
    //                    streamWriter.Close();
    //                }
    //                ++Count;
    //            }
    //        }
    //        catch
    //        {
    //            retVal = false;
    //        }
    //        finally
    //        {
    //            // ZIP 파일 스트림 종료 
    //            zipInputStream.Close();
    //        }

    //        // ZIP파일 삭제를 원할 경우 파일 삭제. 
    //        if (isDeleteZipFile)
    //            try
    //            {
    //                File.Delete(zipFilePath);
    //            }
    //            catch { }
    //    }

    //    return retVal;
    //    retVal = true;
    //}
}