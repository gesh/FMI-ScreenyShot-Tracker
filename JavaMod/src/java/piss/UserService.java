package piss;

import java.awt.AWTException;
import java.awt.Rectangle;
import java.awt.Robot;
import java.awt.Toolkit;
import java.awt.image.BufferedImage;
import java.io.BufferedReader;
import java.io.ByteArrayOutputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.List;
import java.util.Map;
import java.util.logging.Level;
import java.util.logging.Logger;
import javax.imageio.IIOImage;
import javax.imageio.ImageIO;
import javax.imageio.ImageWriteParam;
import javax.imageio.ImageWriter;
import javax.imageio.stream.MemoryCacheImageOutputStream;
import javax.json.Json;
import javax.json.JsonObject;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Context;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.MultivaluedMap;
import javax.ws.rs.core.UriInfo;
import org.apache.commons.codec.binary.Base64;

@Path("/service")
public class UserService {
    
    @GET
    @Path("/screenshot")
    @Produces(MediaType.TEXT_PLAIN)
    public String getScreenshot(@Context UriInfo uriInfo) {
        
        // 1) check for configuration parameters
        MultivaluedMap<String,String> parMap = uriInfo.getQueryParameters();
//      if (!parMap.isEmpty()) {
//            
//            String params = "";
//            for (Map.Entry<String, List<String>> entry : parMap.entrySet()) {
//                params += entry.getKey() + " / " + entry.getValue();
//            }
//            
//            return "What to do with these params ? " + params;
//        }
        
        // 2) take screenshot
        // possibly modify quality + resolution + other img operations
        
        byte[] scrFileBytes = takeScreenShot(parMap);
        
        // 3) encode to base64
        scrFileBytes = Base64.encodeBase64(scrFileBytes);
        
        // send img to Azure API and get modified img
        String imgStrBase64 = putWatermarkRemotely(new String(scrFileBytes));
        
        // TODO: send result to PHP client 
        if (imgStrBase64.isEmpty()) {
            return "response it empty";
        }
        return imgStrBase64;
    }
    
    private byte[] takeScreenShot(MultivaluedMap<String,String> parMap) {
        
        ImageWriter writer = ImageIO.getImageWritersByFormatName("jpeg").next();
        ImageWriteParam writeParam = writer.getDefaultWriteParam();
        
        if (parMap.get("quality") != null && parMap.get("quality").get(0).length() > 0) {
            writeParam.setCompressionMode(ImageWriteParam.MODE_EXPLICIT);
            writeParam.setCompressionQuality(Float.parseFloat(parMap.get("quality").get(0)) / 100);
            System.out.println("Using quality: " + Float.parseFloat(parMap.get("quality").get(0)) / 100);
        }
        
        byte[] imgBytes = null;
        
        try {
            Rectangle screenRect = new Rectangle(Toolkit.getDefaultToolkit().getScreenSize());
            BufferedImage capture = new Robot().createScreenCapture(screenRect);
            
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            writer.setOutput(new MemoryCacheImageOutputStream(baos));
            //baos.flush();
            
            IIOImage img = new IIOImage(capture, null, null);
            writer.write(null, img, writeParam);
            writer.dispose();
            
            imgBytes = baos.toByteArray();
            baos.close();
            
        } catch (AWTException | IOException ex) {
            Logger.getLogger(UserService.class.getName()).log(Level.SEVERE, null, ex);
        }
        
        return imgBytes;
    }
    
    private String putWatermarkRemotely(String imgStrBase64) {
        
        StringBuilder resp = new StringBuilder();
        
        try {
            URL url = new URL("http://piss-image-watermark.azurewebsites.net/api/image");
            
            JsonObject jsonObj = Json.createObjectBuilder()
                                .add("ImageData", imgStrBase64)
                                .build();
            
            HttpURLConnection conn = (HttpURLConnection) url.openConnection();
            conn.setRequestMethod("POST");
            conn.setRequestProperty("Content-Type", "application/json");
            conn.setUseCaches(false);
            conn.setDoInput(true);
            conn.setDoOutput(true);
            
            // Send request
            DataOutputStream dos = new DataOutputStream(conn.getOutputStream());
            dos.writeBytes(jsonObj.toString());
            dos.flush();
            dos.close();
            
            // Get Response	
            InputStream is = conn.getInputStream();
            BufferedReader br = new BufferedReader(new InputStreamReader(is));
            
            String line;
            while ((line = br.readLine()) != null) {
                resp.append(line);
                resp.append('\r');
            }
            br.close();
            
        } catch (MalformedURLException ex) {
            Logger.getLogger(UserService.class.getName()).log(Level.SEVERE, null, ex);
        } catch (IOException ex) {
            Logger.getLogger(UserService.class.getName()).log(Level.SEVERE, null, ex);
        }
        
        System.out.println(resp.toString());
        return resp.toString();
    }
}