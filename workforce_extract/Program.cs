using System;
using System.Data.SqlClient;
using System.IO;
using System.Xml.Linq;
using System.Globalization;

namespace workforce_extract
{
    class Program
    {
        static void Main(string[] args)
        {
            using (
                var conn = new SqlConnection("Server=0,1433;Database=0;User ID=0;Password= 0;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;")
                )
            {
                conn.Open();
                bool quit = false;
                string choice;
                string workforce = "default";
                SqlCommand cmd = new SqlCommand();
                Console.WriteLine("<---------------------");
                Console.WriteLine("       Workforce");
                Console.WriteLine("--------------------->");

                while (!quit)
                {
                    Console.WriteLine("Select by Workforce type: Region or Center?");
                    string wfType = Console.ReadLine();
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    wfType = textInfo.ToLower(wfType);
                    workforce = wfType;
                    if (wfType == "region")
                    {
                        Console.WriteLine("Select by code, date, both, or none?");
                        choice = Console.ReadLine();
                        switch (choice)
                        {
                            case "code":
                                Console.WriteLine("Select by code1 or code2?");
                                string codeCol = Console.ReadLine();
                                Console.WriteLine("Enter desired code");
                                string code = Console.ReadLine();
                                cmd = new SqlCommand("SELECT * FROM WFRegion WHERE (WFRegion.wfr_" + @codeCol + "='" + @code + "') FOR XML PATH('workforceRegion'), ROOT('kcpsRegion')", conn);
                                quit = true;
                                break;

                            case "date":
                                Console.WriteLine("Enter desired date using the format MM/DD/YYYY.");
                                string date = Console.ReadLine();
                                cmd = new SqlCommand("SELECT * FROM WFRegion WHERE (WFRegion.wfr_date_created >= '" + @date + "' OR WFRegion.wfr_date_updated >= '" + @date + "') FOR XML PATH('workforceRegion'), ROOT('kcpsRegion')", conn);
                                quit = true;
                                break;

                            case "both":
                                cmd = new SqlCommand("SELECT * FROM WFRegion FOR XML PATH('workforceRegion'), ROOT('kcpsRegion')", conn);
                                quit = true;
                                break;

                            case "none":
                                cmd = new SqlCommand("SELECT * FROM WFRegion FOR XML PATH('workforceRegion'), ROOT('kcpsRegion')", conn);
                                quit = true;
                                break;

                            default:
                                Console.WriteLine("Unknown Command " + choice);
                                continue;
                        }
                    }
                    else if (wfType == "center")
                    {
                        Console.WriteLine("Select by code, date, both, or none?");
                        choice = Console.ReadLine();
                        switch (choice)
                        {
                            case "code":
                                Console.WriteLine("Select by code1 or code2?");
                                string codeCol = Console.ReadLine();
                                Console.WriteLine("Enter desired code");
                                string code = Console.ReadLine();
                                cmd = new SqlCommand("SELECT * FROM WFCenter WHERE (WFCenter.wfc_" + @codeCol + "='" + @code + "') FOR XML PATH('workforceCenter'), ROOT('kcpsCenter')", conn);
                                quit = true;
                                break;

                            case "date":
                                Console.WriteLine("Enter desired date using the format MM/DD/YYYY.");
                                string date = Console.ReadLine();
                                cmd = new SqlCommand("SELECT * FROM WFCenter WHERE (WFCenter.wfc_date_created >= '" + @date + "' OR WFCenter.wfc_date_updated >= '" + @date + "') FOR XML PATH('workforceCenter'), ROOT('kcpsCenter')", conn);
                                quit = true;
                                break;

                            case "both":
                                cmd = new SqlCommand("SELECT * FROM WFCenter FOR XML PATH('workforceCenter'), ROOT('kcpsCenter')", conn);
                                quit = true;
                                break;

                            case "none":
                                cmd = new SqlCommand("SELECT * FROM WFCenter FOR XML PATH('workforceCenter'), ROOT('kcpsCenter')", conn);
                                quit = true;
                                break;

                            default:
                                Console.WriteLine("Unknown Command " + choice);
                                continue;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unknown Command " + wfType);
                        continue;
                    }
                }
                using (cmd)
                {
                    using (var reader = cmd.ExecuteXmlReader())
                    {
                        var doc = new XDocument();
                        try
                        {
                            doc = XDocument.Load(reader);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine("There are no entries that match the given parameters.");
                        }
                        if (workforce == "region")
                        {
                            string path = @"WFRegion." + DateTime.Now.ToString("yyyyMMdd") + ".xml";
                            using (var writer = new StreamWriter(path))
                            {
                                XNamespace ns = "link placeholder";
                                var root = new XElement(ns + "kcpsRegion");
                                int count = 0;
                                foreach (var d in doc.Descendants("workforceRegion"))
                                {
                                    //SimplerAES item = new SimplerAES();
                                    //string dhash = item.Decrypt((string)d.Element("password"));
                                    string type = "REG";
                                    string name;
                                    string street;
                                    string street2;
                                    string city;
                                    string state = "FL";
                                    string postal;
                                    string regid;
                                    string delete;

                                    /*Delete flag modification*/
                                    if ((string)d.Element("wfr_delete_flag") == "Y")
                                    {
                                        delete = "1";
                                    }
                                    else
                                    {
                                        delete = "0";
                                    }

                                    /*Institution name check for null*/
                                    if ((string)d.Element("wfr_name") == null)
                                    {
                                        name = string.Empty;
                                    }
                                    else
                                    {
                                        name = (string)d.Element("wfr_name");
                                    }
                                    /*street check for null*/
                                    if ((string)d.Element("wfr_streetLine2") == null)
                                    {
                                        street2 = string.Empty;
                                        street = ((string)d.Element("wfr_streetLine1") + street2);
                                    }
                                    else
                                    {
                                        street2 = (string)d.Element("wfr_streetLine2");
                                        street = ((string)d.Element("wfr_streetLine1") + ", " + street2);
                                    }

                                    /*city check for null*/
                                    if ((string)d.Element("wfr_city") == null)
                                    {
                                        city = string.Empty;
                                    }
                                    else
                                    {
                                        city = (string)d.Element("wfr_city");
                                    }

                                    /*stateProvince check for null*/
                                    if ((string)d.Element("wfr_stateProvince") == null)
                                    {
                                        state = string.Empty;
                                    }
                                    else
                                    {
                                        state = (string)d.Element("wfr_stateProvince");
                                    }

                                    /*postal check for null*/
                                    if ((string)d.Element("wfr_postalCode") == null)
                                    {
                                        postal = string.Empty;
                                    }
                                    else
                                    {
                                        postal = (string)d.Element("wfr_postalCode");
                                    }
                                    /*reg id check for null*/
                                    if ((string)d.Element("wfr_regid") == null)
                                    {
                                        regid = string.Empty;
                                    }
                                    else
                                    {
                                        regid = (string)d.Element("wfr_regid");
                                    }
                                    count++;
                                    root.Add(new XElement(ns + "workforceRegion",
                                                new XElement(ns + "type", type),
                                                new XElement(ns + "directory",
                                                    new XElement(ns + "institutionName", name)
                                                        ),
                                                new XElement(ns + "addressList",
                                                    new XElement(ns + "address",
                                                        new XElement(ns + "street",
                                                            new XElement(ns + "line1", street)
                                                            ),
                                                        new XElement(ns + "city", city),
                                                        new XElement(ns + "stateProvince", state),
                                                        new XElement(ns + "postalCode", postal)
                                                            )
                                                    ),
                                                new XElement(ns + "access",
                                                    new XElement(ns + "WFREGID", regid)
                                                    ),
                                                    //),
                                                new XElement(ns + "delete", delete)
                                                )
                                                );
                                }
                                root.Save(writer);
                                Console.WriteLine("" + count + " staff records written");
                                Console.ReadLine();
                            }
                        }
                        else if (workforce == "center")
                        {
                            string path = @"WFCenter." + DateTime.Now.ToString("yyyyMMdd") + ".xml";
                            using (var writer = new StreamWriter(path))
                            {
                                XNamespace ns = "link placeholder";
                                var root = new XElement(ns + "kcpsCenter");
                                int count = 0;
                                foreach (var d in doc.Descendants("workforceCenter"))
                                {
                                    //SimplerAES item = new SimplerAES();
                                    //string dhash = item.Decrypt((string)d.Element("password"));
                                    string type = "WFC";
                                    string name;
                                    string street;
                                    string street2;
                                    string city;
                                    string state = "FL";
                                    string postal;
                                    string county;
                                    string id;
                                    string regid;
                                    string delete;
                                    /*Delete flag modification*/
                                    if ((string)d.Element("wfc_delete_flag") == "Y")
                                    {
                                        delete = "1";
                                    }
                                    else
                                    {
                                        delete = "0";
                                    }

                                    /*Institution name check for null*/
                                    if ((string)d.Element("wfc_name") == null)
                                    {
                                        name = string.Empty;
                                    }
                                    else
                                    {
                                        name = (string)d.Element("wfc_name");
                                    }
                                    /*street check for null*/
                                    if ((string)d.Element("wfc_streetLine2") == null)
                                    {
                                        street2 = string.Empty;
                                        street = ((string)d.Element("wfc_streetLine1") + street2);
                                    }
                                    else
                                    {
                                        street2 = (string)d.Element("wfc_streetLine2");
                                        street = ((string)d.Element("wfc_streetLine1") + ", " + street2);
                                    }

                                    /*city check for null*/
                                    if ((string)d.Element("wfc_city") == null)
                                    {
                                        city = string.Empty;
                                    }
                                    else
                                    {
                                        city = (string)d.Element("wfc_city");
                                    }

                                    /*stateProvince check for null*/
                                    if ((string)d.Element("wfc_stateProvince") == null)
                                    {
                                        state = string.Empty;
                                    }
                                    else
                                    {
                                        state = (string)d.Element("wfc_stateProvince");
                                    }

                                    /*postal check for null*/
                                    if ((string)d.Element("wfc_postalCode") == null)
                                    {
                                        postal = string.Empty;
                                    }
                                    else
                                    {
                                        postal = (string)d.Element("wfc_postalCode");
                                    }
                                    /*id name check for null*/
                                    if ((string)d.Element("wfc_id") == null)
                                    {
                                        id = string.Empty;
                                    }
                                    else
                                    {
                                        id = (string)d.Element("wfc_id");
                                    }
                                    /*county check for null*/
                                    if ((string)d.Element("wfc_county") == null)
                                    {
                                        county = string.Empty;
                                    }
                                    else
                                    {
                                        county = (string)d.Element("wfc_county");
                                    }
                                    /*reg id check for null*/
                                    if ((string)d.Element("wfc_regid") == null)
                                    {
                                        regid = string.Empty;
                                    }
                                    else
                                    {
                                        regid = (string)d.Element("wfc_regid");
                                    }
                                    count++;
                                    root.Add(new XElement(ns + "workforceCenter",
                                                new XElement(ns + "type", type),
                                                new XElement(ns + "directory",
                                                    new XElement(ns + "institutionName", name)
                                                    ),
                                                new XElement(ns + "addressList",
                                                    new XElement(ns + "address",
                                                        new XElement(ns + "street",
                                                            new XElement(ns + "line1", street)
                                                            ),
                                                        new XElement(ns + "city", city),
                                                        new XElement(ns + "stateProvince", state),
                                                        new XElement(ns + "postalCode", postal),
                                                        new XElement(ns + "county", county)
                                                            )
                                                    ),
                                                new XElement(ns + "access",
                                                    new XElement(ns + "WFID", id),
                                                    new XElement(ns + "WFREGID", regid)
                                                    ),
                                                //),
                                                new XElement(ns + "delete", delete)
                                                )
                                                );
                                }
                                root.Save(writer);
                                Console.WriteLine("" + count + " staff records written");
                                Console.ReadLine();
                            }

                        }


                    }
                }
            }

        }
    }
}
