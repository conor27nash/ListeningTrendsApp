// Hash a string into a numeric value
function hashString(str) {
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    hash = str.charCodeAt(i) + ((hash << 5) - hash);
  }
  return hash;
}

// Generate a consistent, bright, dark-mode-friendly colour for each artist
export function getArtistColour(artistName) {
  if (!artistName) return '#999';
  
  const hash = hashString(artistName.toLowerCase());
  
  // Spread hues evenly across the 360° colour wheel
  const hue = Math.abs(hash) % 360;
  
  // Saturation and lightness tuned for dark backgrounds
  const saturation = 65; // %
  const lightness = 55; // %

  return `hsl(${hue}, ${saturation}%, ${lightness}%)`;
}