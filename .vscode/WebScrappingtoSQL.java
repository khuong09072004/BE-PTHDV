import org.jsoup.Jsoup;
import org.jsoup.nodes.Document;
import org.jsoup.nodes.Element;
import org.jsoup.select.Elements;
import java.math.BigDecimal;

import java.io.IOException;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.SQLException;

public class WebScrappingtoSQL {
    public static void main(String[] args) {
        try {
            // Lặp qua các trang của Books to Scrape
            for (int i = 1; i <= 2; i++) {
                String pageUrl = "https://books.toscrape.com/catalogue/page-" + i + ".html";
                Document doc = Jsoup.connect(pageUrl).get();

                // Tìm tất cả các sách trên trang
                Elements books = doc.select(".product_pod h3 a");

                // Lặp qua từng sách và lấy dữ liệu từ trang chi tiết
                for (Element book : books) {
                    String bookUrl = "https://books.toscrape.com/catalogue/" + book.attr("href");
                    scrapeBookDetails(bookUrl);
                }
            }

        } catch (IOException e) {
            System.out.println("Error when connecting to the website:");
            e.printStackTrace();
        }
    }

    private static void scrapeBookDetails(String url) {
        String jdbcURL = "jdbc:mysql://localhost:3306/data?useSSL=false&allowPublicKeyRetrieval=true";
        String username = "root";
        String password = "Duong1997@";

        try (Connection connection = DriverManager.getConnection(jdbcURL, username, password)) {
            Document doc = Jsoup.connect(url).get();
            // Lấy các thông tin chi tiết của sách
            String upc = doc.select("table tr:contains(UPC) td").text();
            String title = doc.select("div.product_main h1").text();
            String genre = doc.select("ul.breadcrumb li:nth-child(3) a").text();
            String priceText = doc.select("p.price_color").first().text().replace("£", "").trim();
            BigDecimal price = new BigDecimal(priceText);
            String imgSrc = "https://books.toscrape.com/" + doc.select(".item.active img").attr("src");
            String starRating = doc.select("p.star-rating").attr("class").replace("star-rating ", "");
            String stockStatus = doc.select("p.instock.availability").text().contains("In stock") ? "In stock" : "Out of stock";
            String description = doc.select("div#product_description + p").text();
            // Kiểm tra và chuyển đổi số lượng sách còn lại (nếu có)
            int numAvailable = 0;
            String availableText = doc.select("p.instock.availability").text().replaceAll("\\D+", "");
            if (!availableText.isEmpty()) {
                numAvailable = Integer.parseInt(availableText);
            }

            // Lưu thông tin vào cơ sở dữ liệu
            saveToDatabase(connection, upc, title, genre, price, imgSrc, starRating, stockStatus, numAvailable,description);
            System.out.println("Data has been saved to MySQL successfully!");

        } catch (SQLException e) {
            System.out.println("Failed to connect or execute SQL statement:");
            e.printStackTrace();
        } catch (IOException e) {
            System.out.println("Error when getting data from web:");
            e.printStackTrace();
        }
    }

    private static void saveToDatabase(Connection connection, String upc, String title, String genre, BigDecimal price, String imgSrc, String starRating, String stockStatus, int numAvailable,String description) {
        String sql = "INSERT INTO books (id, upc, title, genre, price, img_src, star_rating, instock, number_available, description) " +
                     "VALUES (NULL, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

        try (PreparedStatement statement = connection.prepareStatement(sql)) {
            statement.setString(1, upc);
            statement.setString(2, title);
            statement.setString(3, genre);
            statement.setBigDecimal(4, price);
            statement.setString(5, imgSrc);
            statement.setString(6, starRating);
            statement.setString(7, stockStatus);
            statement.setInt(8, numAvailable);
            statement.setString(9,description);
            int rows = statement.executeUpdate();
            if (rows > 0) {
                System.out.println("Data inserted successfully for book: " + title);
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
    }
}
