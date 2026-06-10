export const initialWines = [
  {
    id: 1,
    name: 'Château Margaux',
    domain: 'Château Margaux',
    year: 2018,
    rank: 5,
    description: "Premier Grand Cru Classé de Bordeaux. Un vin d'une élégance et complexité exceptionnelles, aux tanins soyeux et à la longue finale.",
    imageUrl: 'https://placehold.co/300x200/722f37/white?text=Ch.+Margaux',
    drinkFromYear: 2026,
    drinkToYear: 2050,
    cepages: [
      { name: 'Cabernet Sauvignon', percentage: 75 },
      { name: 'Merlot', percentage: 20 },
      { name: 'Petit Verdot', percentage: 5 }
    ]
  },
  {
    id: 2,
    name: 'Gevrey-Chambertin',
    domain: 'Domaine Rossignol-Trapet',
    year: 2019,
    rank: 4,
    description: 'Un Bourgogne rouge de caractère, aux arômes de fruits rouges, de sous-bois et épices. Belle structure tannique.',
    imageUrl: 'https://placehold.co/300x200/8B1A1A/white?text=Gevrey',
    drinkFromYear: 2023,
    drinkToYear: 2035,
    cepages: [
      { name: 'Pinot Noir', percentage: 100 }
    ]
  },
  {
    id: 3,
    name: 'Puligny-Montrachet',
    domain: 'Domaine Leflaive',
    year: 2020,
    rank: 5,
    description: 'Grand Cru de Bourgogne blanc. Notes de fleurs blanches, agrumes et minéralité. Pureté cristalline.',
    imageUrl: 'https://placehold.co/300x200/c8a951/333?text=Puligny',
    drinkFromYear: 2024,
    drinkToYear: 2035,
    cepages: [
      { name: 'Chardonnay', percentage: 100 }
    ]
  },
  {
    id: 4,
    name: 'Châteauneuf-du-Pape',
    domain: 'Château Rayas',
    year: 2017,
    rank: 5,
    description: "L'un des meilleurs vins de la Vallée du Rhône. Grenache dominant, richesse et profondeur remarquables.",
    imageUrl: 'https://placehold.co/300x200/6B1A2A/white?text=CDP',
    drinkFromYear: 2022,
    drinkToYear: 2040,
    cepages: [
      { name: 'Grenache', percentage: 90 },
      { name: 'Cinsault', percentage: 10 }
    ]
  },
  {
    id: 5,
    name: 'Sancerre',
    domain: 'Henri Bourgeois',
    year: 2021,
    rank: 3,
    description: 'Sauvignon Blanc de la Loire. Vif, minéral, notes de silex et agrumes. À boire jeune.',
    imageUrl: 'https://placehold.co/300x200/d4c27a/333?text=Sancerre',
    drinkFromYear: 2022,
    drinkToYear: 2026,
    cepages: [
      { name: 'Sauvignon Blanc', percentage: 100 }
    ]
  },
  {
    id: 6,
    name: 'Barossa Valley Shiraz',
    domain: 'Penfolds',
    year: 2019,
    rank: 4,
    description: 'Shiraz australien puissant et opulent. Notes de mûre, poivre noir et chocolat. Tanins généreux.',
    imageUrl: 'https://placehold.co/300x200/4A0000/white?text=Barossa',
    drinkFromYear: 2024,
    drinkToYear: 2038,
    cepages: [
      { name: 'Syrah', percentage: 100 }
    ]
  },
  {
    id: 7,
    name: 'Pouilly-Fumé',
    domain: 'Didier Dagueneau',
    year: 2020,
    rank: 4,
    description: 'Référence du Sauvignon Blanc. Intense, complexe, minéralité unique issue du silex de la Loire.',
    imageUrl: 'https://placehold.co/300x200/e8d87a/333?text=Pouilly',
    drinkFromYear: 2023,
    drinkToYear: 2030,
    cepages: [
      { name: 'Sauvignon Blanc', percentage: 100 }
    ]
  },
  {
    id: 8,
    name: 'Rioja Gran Reserva',
    domain: 'Marqués de Murrieta',
    year: 2015,
    rank: 4,
    description: 'Rioja classique avec longues années de vieillissement. Élégant, complexe, tanins fondus.',
    imageUrl: 'https://placehold.co/300x200/8B0000/white?text=Rioja',
    drinkFromYear: 2020,
    drinkToYear: 2030,
    cepages: [
      { name: 'Tempranillo', percentage: 80 },
      { name: 'Garnacha', percentage: 15 },
      { name: 'Mazuelo', percentage: 5 }
    ]
  },
  {
    id: 9,
    name: 'Chablis Premier Cru',
    domain: 'Domaine William Fèvre',
    year: 2021,
    rank: 3,
    description: 'Chardonnay non boisé de belle fraîcheur. Notes iodées et minéralité calcaire typiques de Chablis.',
    imageUrl: 'https://placehold.co/300x200/c5b97a/333?text=Chablis',
    drinkFromYear: 2023,
    drinkToYear: 2029,
    cepages: [
      { name: 'Chardonnay', percentage: 100 }
    ]
  },
  {
    id: 10,
    name: 'Amarone della Valpolicella',
    domain: 'Allegrini',
    year: 2016,
    rank: 5,
    description: 'Vin italien de grande noblesse produit à partir de raisins séchés. Dense, riche, finale interminable.',
    imageUrl: 'https://placehold.co/300x200/3D0000/white?text=Amarone',
    drinkFromYear: 2022,
    drinkToYear: 2045,
    cepages: [
      { name: 'Corvina', percentage: 70 },
      { name: 'Rondinella', percentage: 25 },
      { name: 'Molinara', percentage: 5 }
    ]
  }
]

export const initialRecipes = [
  {
    id: 1,
    name: 'Boeuf Bourguignon',
    description: 'Le grand classique de la cuisine française. Boeuf mijoté dans le vin rouge de Bourgogne avec champignons et lardons.',
    imageUrl: 'https://placehold.co/300x200/5D4037/white?text=Bourguignon',
    recipeType: 'Main',
    ingredients: [
      '1,5 kg de boeuf à braiser',
      '1 bouteille de Bourgogne rouge',
      '200 g de lardons',
      '300 g de champignons',
      '2 oignons',
      '3 carottes',
      'Bouquet garni',
      'Sel, poivre'
    ],
    instructions: "1. Couper le boeuf en gros cubes et faire mariner dans le vin avec les légumes pendant 12h.\n2. Faire revenir les lardons et les légumes.\n3. Ajouter le boeuf et la marinade.\n4. Laisser mijoter 3h à feu doux.\n5. Ajouter les champignons 30 min avant la fin.",
    pairings: [
      { wineId: 2, notes: 'Un Gevrey-Chambertin ou autre Bourgogne rouge est idéal.' }
    ]
  },
  {
    id: 2,
    name: 'Plateau de Fromages',
    description: 'Sélection de fromages affinés avec pain aux noix et raisin. Idéal pour prolonger un bon repas.',
    imageUrl: 'https://placehold.co/300x200/D4A017/333?text=Fromages',
    recipeType: 'Starter',
    ingredients: [
      'Comté affiné 24 mois',
      'Brie de Meaux',
      'Roquefort',
      'Chèvre frais',
      'Pain aux noix',
      'Raisin blanc',
      'Confiture de figues'
    ],
    instructions: "1. Sortir les fromages du réfrigérateur 1h avant.\n2. Disposer harmonieusement sur un plateau en bois.\n3. Compléter avec le pain, le raisin et les condiments.",
    pairings: [
      { wineId: 3, notes: 'Un Puligny-Montrachet se marie parfaitement avec le Comté.' },
      { wineId: 2, notes: 'Un rouge léger de Bourgogne pour les fromages à pâte molle.' }
    ]
  },
  {
    id: 3,
    name: 'Tartare de Saumon',
    description: 'Saumon frais haché avec échalotes, câpres et citron. Simple, frais et élégant.',
    imageUrl: 'https://placehold.co/300x200/c46a4e/white?text=Saumon',
    recipeType: 'Starter',
    ingredients: [
      '400 g de saumon frais',
      '2 échalotes',
      '2 c. à soupe de câpres',
      '1 citron',
      'Aneth frais',
      "Huile d'olive",
      'Sel, poivre'
    ],
    instructions: "1. Hacher le saumon au couteau en petits cubes.\n2. Mélanger avec les échalotes émincées et les câpres.\n3. Assaisonner avec jus de citron, huile d'olive, sel et poivre.\n4. Dresser en cercle et garnir d'aneth.",
    pairings: [
      { wineId: 5, notes: 'Un Sancerre ou tout Sauvignon Blanc frais.' },
      { wineId: 7, notes: 'Le Pouilly-Fumé apporte une belle minéralité.' }
    ]
  },
  {
    id: 4,
    name: 'Magret de Canard aux Cerises',
    description: 'Magret de canard poêlé avec sauce aux cerises et purée de panais. Un accord terre et fruit.',
    imageUrl: 'https://placehold.co/300x200/B22222/white?text=Magret',
    recipeType: 'Main',
    ingredients: [
      '2 magrets de canard',
      '300 g de cerises',
      '2 panais',
      '100 ml de bouillon de volaille',
      '50 ml de Cognac',
      'Thym',
      'Beurre, sel, poivre'
    ],
    instructions: "1. Quadriller la peau des magrets et assaisonner.\n2. Cuire côté peau 10 min puis retourner 5 min.\n3. Déglacer au Cognac.\n4. Ajouter les cerises et le bouillon, réduire.\n5. Servir avec la purée de panais.",
    pairings: [
      { wineId: 4, notes: "Un Châteauneuf-du-Pape s'accorde à merveille." },
      { wineId: 1, notes: 'Un grand Bordeaux pour une occasion spéciale.' }
    ]
  },
  {
    id: 5,
    name: 'Tiramisu au Café',
    description: 'Dessert italien classique. Biscuits imbibés de café, mascarpone crémeux et cacao amer.',
    imageUrl: 'https://placehold.co/300x200/6F4E37/white?text=Tiramisu',
    recipeType: 'Dessert',
    ingredients: [
      '500 g de mascarpone',
      '4 oeufs',
      '100 g de sucre',
      '400 ml de café fort refroidi',
      '24 biscuits à la cuillère',
      'Cacao en poudre non sucré',
      '4 c. à soupe de Marsala (optionnel)'
    ],
    instructions: "1. Séparer les blancs des jaunes d'oeufs.\n2. Battre les jaunes avec le sucre jusqu'à blanchiment.\n3. Incorporer le mascarpone.\n4. Monter les blancs en neige et incorporer délicatement.\n5. Imbiber les biscuits dans le café.\n6. Alterner couches de biscuits et crème.\n7. Réfrigérer 6h minimum. Saupoudrer de cacao avant de servir.",
    pairings: [
      { wineId: 10, notes: "Un Amarone ou Recioto della Valpolicella pour les amateurs de vins doux." }
    ]
  }
]

export function initMockData() {
  if (localStorage.getItem('wca_initialized')) return

  localStorage.setItem('wca_wines', JSON.stringify(initialWines))
  localStorage.setItem('wca_recipes', JSON.stringify(initialRecipes))

  const demoUser = { id: 1, email: 'demo@winecellar.com', password: 'password123' }
  localStorage.setItem('wca_users', JSON.stringify([demoUser]))

  localStorage.setItem('wca_cellar_1', JSON.stringify([
    { wineId: 1, bottleCount: 3 },
    { wineId: 2, bottleCount: 6 },
    { wineId: 5, bottleCount: 2 }
  ]))

  localStorage.setItem('wca_initialized', 'true')
}
