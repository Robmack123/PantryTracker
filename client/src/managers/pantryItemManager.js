const apiUrl = "/api/pantryitem";

export const getPantryItems = () => {
  return fetch(`${apiUrl}`, {
    method: "GET",
    credentials: "include", // Ensures the user's session cookie is sent with the request
    headers: {
      "Content-Type": "application/json",
    },
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to fetch pantry items.");
    }
    return res.json();
  });
};
