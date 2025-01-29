const API_BASE_URL = "http://3.147.46.97:5000"; // Replace with your EC2 IP
const apiUrl = `${API_BASE_URL}/api/category`;

export const getCategories = () => {
  return fetch(apiUrl, {
    method: "GET",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
    },
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to fetch categories.");
    }
    return res.json();
  });
};
