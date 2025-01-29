const apiUrl = "/api/category";

export const getCategories = () => {
  return fetch(`${apiUrl}`, {
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
