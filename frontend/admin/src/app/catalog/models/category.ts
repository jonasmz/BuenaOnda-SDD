export interface Category {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
}

export interface CategoryInput {
  name: string;
  description: string | null;
}
