const API_BASE_URL = "http://3.147.46.97:5000"; // Replace with your EC2 IP
const apiUrl = `${API_BASE_URL}/api/pantryitem`;

export const getRecentActivity = () => {
  return fetch(`${apiUrl}/recent-activity`, {
    method: "GET",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
    },
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to fetch recent activity.");
    }
    return res.json();
  });
};
