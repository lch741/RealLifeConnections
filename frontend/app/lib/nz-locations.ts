export type NzLocation = { region: string; cities: string[] };

const nzRegionDistrictSuburbs: Record<string, Record<string, string[]>> = {
  'Northland': {
    'Far North': [
      'Ahipara', 'Awanui', 'Cable Bay', 'Cape Reinga', 'Cavalli Islands', 'Coopers Beach', 'Haruru',
      'Henderson Bay', 'Herekino', 'Hokianga', 'Hokianga Harbour', 'Horeke', 'Houhora', 'Kaeo', 'Kaikohe',
      'Kaimaumau', 'Kaitaia', 'Karetu', 'Karikari Peninsula', 'Kawakawa', 'Kerikeri', 'Kohukohu', 'Mangonui',
      'Mitimiti', 'Moerewa', 'Ohaeawai', 'Okaihau', 'Omanaia', 'Omapere', 'Opononi', 'Paihia', 'Pakaraka',
      'Panguru', 'Pukenui', 'Rangiahua', 'Rawene', 'Russell', 'Taheke', 'Takahue', 'Tokerau Beach',
      'Totara North', 'Towai', 'Waimamaku', 'Waipapa', 'Whangape Harbour', 'Whangaroa'
    ],
    'Kaipara': [
      'Arapohue', 'Baylys Beach', 'Dargaville', 'Kaiwaka', 'Mangawhai', 'Mangawhai Heads', 'Mangawhare',
      'Matakohe', 'Maungaturoto', 'Paparoa', 'Ruawai', 'Tangiteroria', 'Te Kopuru', 'Tinopai', 'Waipoua'
    ],
    'Whangarei': [
      'Bream Bay', 'Central Whangarei', 'Glenbervie', 'Hikurangi', 'Hukerenui', 'Kamo', 'Kauri', 'Kensington',
      'Kokopu', 'Mairtown', 'Mangapai', 'Marsden Point', 'Marua', 'Matapouri', 'Maungakaramea', 'Maungatapere',
      'Maunu', 'Morningside', 'Ngunguru', 'Oakleigh', 'Oakura Coast', 'One Tree Point', 'Onerahi', 'Otaika',
      'Otangarei', 'Parahaki', 'Parua Bay', 'Pataua', 'Pataua North', 'Pataua South', 'Portland', 'Raumanga',
      'Regent', 'Riverside', 'Raukaka', 'Ruatangata', 'Sherwood Rise', 'Springs Flat', 'Tamaterau', 'Tikipunga',
      'Titoki', 'Toetoe', 'Tutukaka', 'Vinetown', 'Waiotira', 'Waipu', 'Waipu Cove', 'Whananaki',
      'Whangarei Heads', 'Whangaruru', 'Whareora', 'Whau Valley', 'Woodhill'
    ]
  },
  'Auckland': {
    'Auckland City': [
      'Avondale', 'Balmoral', 'Blockhouse Bay', 'City Centre', 'Eden Terrace', 'Ellerslie', 'Epsom',
      'Freemans Bay', 'Glen Innes', 'Glendowie', 'Grafton', 'Great Barrier Island', 'Greenlane', 'Grey Lynn',
      'Herne Bay', 'Hillsborough', 'Kingsland', 'Kohimarama', 'Lynfield', 'Meadowbank', 'Mission Bay',
      'Morningside', 'Mount Albert', 'Mount Eden', 'Mount Roskill', 'Mount Wellington', 'New Windsor',
      'Newmarket', 'Newtown', 'One Tree Hill', 'Onehunga', 'Orakei', 'Oranga', 'Otahuhu', 'Owairaka', 'Panmure',
      'Parnell', 'Penrose', 'Point Chevalier', 'Point England', 'Ponsonby', 'Rakino Island', 'Remuera',
      'Royal Oak', 'Saint Heliers', 'Saint Johns', 'Saint Johns Park', 'Saint Marys Bay', 'Sandringham', 'Tamaki',
      'Te Papapa', 'Three Kings', 'Wai O Taiki Bay', 'Waikowhai', 'Waterview', 'Wesley', 'Western Springs', 'Westmere'
    ],
    'Franklin': [
      'Aka Aka', 'Ararimu', 'Awhitu', 'Bombay', 'Buckland', 'Clarks Beach', 'Glenbrook', 'Harrisville', 'Hunua',
      'Kaiaua', 'Karaka', 'Mauku', 'Onewhero', 'Patumahoe', 'Pokeno', 'Port Waikato', 'Pukekawa', 'Pukekohe',
      'Pukekohe East', 'Puni', 'Ramarama', 'Tuakau', 'Waiau Pa', 'Waiuku'
    ],
    'Manakau City': [
      'Alfriston', 'Beachlands', 'Botany Downs', 'Brookby', 'Bucklands Beach', 'Burswood', 'Chapel Downs',
      'City Centre', 'Clendon Park', 'Clevedon', 'Clover Park', 'Cockle Park', 'Cumbria Downs', 'Dannemora',
      'East Tamaki', 'Eastern Beach', 'Farm Cove', 'Favona', 'Flat Bush', 'Golflands', 'Goodwood Heights',
      'Half Moon Bay', 'Highland Park', 'Hill Park', 'Howick', 'Huntington Park', 'Kawakawa Bay', 'Mahia Park',
      'Mangere', 'Mangere Bridge', 'Manukau', 'Manukau Heights', 'Manurewa', 'Maraetai', 'Mellons Bay',
      'Northpark', 'Omana Beach', 'Orere Point', 'Otara', 'Pakuranga', 'Papatoetoe', 'Point View Park', 'Puhinui',
      'Randwick Park', 'Somerville', 'Sunnyhills', 'The Gardens', 'Totara Heights', 'Wattle Downs', 'Weymouth',
      'Whitford', 'Wiri'
    ],
    'North Shore City': [
      'Albany', 'Bayswater', 'Beach Haven', 'Belmont', 'Birkdale', 'Birkenhead', 'Browns Bay', 'Campbells Bay',
      'Castor Bay', 'Chatswood', 'Cheltenham', 'Chester Park', 'Crown Hill', 'Devonport', 'Forrest Hill',
      'Glenfield', 'Greenhithe', 'Hauraki', 'Hillcrest', 'Mairangi Bay', 'Marlborough', 'Meadowood', 'Milford',
      'Murrays Bay', 'North Harbour', 'Northcote', 'Northcross', 'Okura', 'Paremoremo', 'Pinehill', 'Rothesay Bay',
      'Stanley Bay', 'Sunnynook', 'Takapuna', 'Torbay', 'Unsworth Heights', 'Waiake', 'Wainoni', 'Westlake',
      'Windsor Park'
    ],
    'Papakura': ['Ardmore', 'Conifer Grove', 'Drury', 'Opaheke', 'Pahurehure', 'Papakura', 'Red Hill', 'Takanini'],
    'Rodney': [
      'Algies Bay', 'Arkles Bay', 'Army Bay', 'Big Manly', 'Coatesville', 'Dairy Flat', 'Gulf Harbour',
      'Hatfields Beach', 'Helensville', 'Hobbs Bay', 'Hoteo', 'Huapai', 'Kaipara', 'Kaipara Flats', 'Kaukapakapa',
      'Kawau Island', 'Kumeu', 'Leigh', 'Little Manly', 'Mahurangi', 'Matakana', 'Matakatia Bay', 'Maygrove',
      'Muriwai Beach', 'Muriwai Valley', 'Omaha', 'Omaha Flats', 'Orewa', 'Parakai', 'Parkhurst', 'Point Wells',
      'Puhoi', 'Red Beach', 'Riverhead', 'Sandspit', 'Silverdale', 'Snells Beach', 'Stanmore Bay', 'Stillwater',
      'Streamlands', 'Tapora', 'Taupaki', 'The Grange', 'Tindalls Beach', 'Wade Heads', 'Waikoukou Valley',
      'Waimauku', 'Wainui', 'Waitakere', 'Waitoki', 'Waiwera', 'Warkworth', 'Wellsford', 'Whangaparaoa', 'Woodcocks'
    ],
    'Waiheke Island': [
      'Awaawaroa Bay', 'Blackpool', 'Church Bay', 'Enclosure Bay', 'Hekerua Bay', 'Kennedy Point', 'Little Oneroa',
      'Omiha', 'Oneroa', 'Onetangi', 'Orapiu', 'Ostend', 'Palm Beach', 'Rocky Bay', 'Sandy Bay', 'Surfdale',
      'Te Whau', 'Woodside Bay'
    ],
    'Waitakere City': [
      'Bethells Beach (Te Henga)', 'Cornwallis', 'French Bay', 'Glen Eden', 'Glendene', 'Green Bay', 'Harbour View',
      'Henderson', 'Henderson Valley', 'Herald Island', 'Hobsonville', 'Huia', 'Karekare', 'Kelston', 'Konini',
      'Laingholm', 'Massey East', 'Massey North', 'Massey West', 'Mclaren Park', 'New Lynn', 'Oratia',
      'Palm Heights', 'Piha', 'Ranui', 'Sunnyvale', 'Swanson', 'Te Atatu Peninsula', 'Te Atatu South', 'Titirangi',
      'Waiatarua', 'Waima', 'Waimanu Bay', 'West Harbour', 'Western Heights', 'Whenuapai', 'Wood Bay', 'Woodlands Park'
    ]
  },
  'Waikato': {
    'Hamilton City': [
      'Beerescourt', 'Chartwell', 'Chedworth Park', 'Claudelands', 'Deanwell', 'Dinsdale', 'Enderley', 'Fairfield',
      'Fairview Downs', 'Fitzroy', 'Flagstaff', 'Forest lake', 'Frankton', 'Glenview', 'Hamilton City Central',
      'Hamilton East', 'Hamilton North', 'Hamilton West', 'Harrowfield', 'Hillcrest', 'Horsham Downs', 'Livingstone',
      'Maeroa', 'Melville', 'Nawton', 'Pukete', 'Queenwood', 'Rototuna', 'Saint Andrews', 'Silverdale', 'Te Rapa',
      'Temple View', 'Thornton Estate', 'Western Heights', 'Whitiora'
    ],
    'Hauraki': ['Kerepehi', 'Ngatea', 'Paeroa', 'Turua', 'Waihi', 'Waihi East', 'Waikino', 'Whiritoa'],
    'Matamata-Piako': ['Mangateparu', 'Matamata', 'Morrinsville', 'Patetonga', 'Tahuna', 'Te Aroha', 'Te Poi', 'Waharoa'],
    'Maungatapere': ['Whatitiri'],
    'Otorohanga': ['Kawhia', 'Otorohanga'],
    'South Waikato': ['Hodderville', 'Lichfield', 'Ngatira', 'Putaruru', 'Tirau', 'Tokoroa', 'Wiltsdown'],
    'Taupo': [
      'Acacia Bay', 'Hilltop', 'Invergarry', 'Kinloch', 'Kuratau', 'Lake Taupo', 'Mangakino', 'Motuoapa', 'Nukuhau',
      'Oruanui', 'Rainbow Point', 'Richmond Heights', 'Tauhara', 'Te Rangiita', 'Town Centre', 'Turangi', 'Waipahihi',
      'Waitahanui', 'Wharewaka'
    ],
    'Thames-Coromandel': [
      'Cooks Beach', 'Coromandel', 'Hahei', 'Hot Water Beach', 'Kopu', 'Kuaotunu Beach', 'Matarangi', 'Onemana',
      'Opito', 'Pauanui', 'Tairua', 'Tapu', 'Te Puru', 'Thames', 'Whangamata', 'Whangapoua', 'Whitianga'
    ],
    'Waikato': [
      'Glen Murray', 'Gordonton', 'Horotiu', 'Huntly', 'Kainui', 'Matangi', 'Meremere', 'Newstead', 'Ngaruawahia',
      'Raglan', 'Tamahere', 'Taupiri', 'Te Kauwhata', 'Te Kowhai', 'Waikato Surrounds', 'Whatawhata'
    ],
    'Waipa': ['Cambridge', 'Karapiro', 'Kihikihi', 'Leamington', 'Ngahinapouri', 'Ohaupo', 'Pirongia', 'Pukekura', 'Te Awamutu', 'Te Pahu', 'Te Rore'],
    'Waitomo': ['Awakino', 'Piopio', 'Te Kuiti']
  },
  'Bay Of Plenty': {
    'Kawerau': ['Kawerau'],
    'Opotiki': ['Hospital Hill', 'Kutarere', 'Opotiki'],
    'Rotorua': [
      'Atiamuri', 'Awahou', 'Fairy Springs', 'Fenton Park', 'Fordlands', 'Glenholme', 'Hamurana', 'Hannahs Bay',
      'Hillcrest', 'Holdens Bay', 'Horohoro', 'Kaharoa', 'Kawaha Point', 'Koutu', 'Lake Okareka', 'Lake Rotoma',
      'Lake Tarawera', 'Lynmore', 'Mamaku', 'Mangakakahi', 'Maitpo Heights', 'Mourea', 'Ngakuru', 'Ngapuna',
      'Ngongotaha', 'Okere Falls', 'Owhata', 'Pleasant Heights', 'Pukehangi', 'Reporoa', 'Rerewhakaaitu', 'Rotoiti',
      'Rotorua', 'Selwyn Heights', 'Springfield', 'Sunnybrook', 'Te Ngae', 'Tihiotonga', 'Utuhina', 'Waikite',
      'Waikite Valley', 'Westbrook', 'Western Heights', 'Whakarewarewa'
    ],
    'Tauranga City': [
      'Aratiki', 'Avenues', 'Bayfair', 'Bellevue', 'Bethlehem', 'Brookfield', 'Bureta', 'Cambridge Heights',
      'City Centre', 'Gate Pa', 'Greerton', 'Hairini', 'Judea', 'Kairua', 'Matua', 'Maungatapu', 'Mount Maunganui',
      'Ohauiti', 'Omanu', 'Otumoetai', 'Papamoa', 'Papamoa Beach', 'Parkvale', 'Poike', 'Pyes Pa', 'Tauranga South',
      'Tauriko', 'Te Maunga', 'Welcome Bay'
    ],
    'Western Bay of Plenty': [
      'Aongatete', 'Athenree', 'Katikati', 'Lower Kaimai', 'Maketu', 'Omokoroa', 'Oropi', 'Paengaroa', 'Pukehina',
      'Pyes Pa', 'Rangiuru', 'Te Puke', 'Te Puna', 'Waihi Beach', 'Whakamaramara'
    ],
    'Whakatane': [
      'Awakaponga', 'Awakeri', 'Coastlands', 'Edgecumbe', 'Galatea', 'Matata', 'Murupara', 'Ohope', 'Onepu',
      'Otakiri', 'Pahou', 'Paroa', 'Poroporo', 'Port Ohope', 'Ruatoke North', 'Taneatua', 'Te Teko', 'Thornton',
      'Waimana', 'Whakatane'
    ]
  },
  'Gisborne': {
    'Gisborne': [
      'Awapuni', 'City Centre', 'Elgin', 'Kaiti', 'Mangapapa', 'Manutuke', 'Okitu', 'Outer Kaiti', 'Pahutahi',
      'Riverdale', 'Ruatoria', 'Tamarau', 'Te Hapara', 'Te Karaka', 'Tokomaru Bay', 'Tolaga Bay', 'Wainui', 'Whataupoko'
    ]
  },
  'Hawkes Bay': {
    'Central Hawkes Bay': ['Elsthorpe', 'Ongaonga', 'Otane', 'Patangata', 'Porangahau', 'Takapau', 'Waipawa', 'Waipukurau', 'Wanstead'],
    'Hastings': [
      'Akina', 'Bridge Pa', 'Camberley', 'Clive', 'Crownthorpe', 'Eskdale', 'Fernhill', 'Flaxmere', 'Frimley',
      'Hastings Central', 'Haumoana', 'Havelock North', 'Longlands', 'Mahora', 'Maraekakaho', 'Mayfair', 'Moteo',
      'Omahu', 'Pakipaki', 'Parkvale', 'Patoka', 'Poukawa', 'Puketapu', 'Puketitiri', 'Raukawa', 'Raureka',
      'Rissington', 'Saint Leonards', 'Sherenden', 'Te Awanga', 'Tomoana', 'Twyford', 'Waimaramara', 'Waipatu',
      'Whakatu', 'Whirinaki'
    ],
    'Napier City': [
      'Ahuriri', 'Awatoto', 'Bay View', 'Bluff Hill', 'City Centre', 'Greenmeadows', 'Hospital Hill', 'Maraenui',
      'Marewa', 'Meeanee', 'Mission View', 'Napier South', 'Onekawa', 'Pirimai', 'Poraiti', 'Tamatea', 'Taradale',
      'Westshore'
    ],
    'Wairoa': ['Frasertown', 'Mahia Beach', 'wairoa']
  },
  'Taranaki': {
    'New Plymouth': [
      'Bell Block', 'Brixton', 'Brooklands', 'City Centre', 'Egmont Village', 'Fitzroy', 'Frankleigh Park', 'Glen Avon',
      'Highlands Park', 'Hurdon', 'Hurworth', 'Inglewood', 'Lepperton', 'Mangorei', 'Marfell', 'Merrilands', 'Mokau',
      'Moturoa', 'Oakura', 'Okato', 'Omata', 'Onaero', 'Spotswood', 'Strandon', 'Tariki', 'Tikorangi', 'Urenui',
      'Vogeltown', 'waitara', 'Welbourn', 'Westown', 'Whalers Gate'
    ],
    'South Taranaki': ['Alton', 'Eltham', 'Hawera', 'Manaia', 'Normanby', 'Okaiawa', 'Opunake', 'Patea', 'Pungarehu', 'Tokaora', 'Waitotara', 'Warea', 'Waverley'],
    'Stratford': ['Midhurst', 'Stratford East', 'Stratford West', 'Toko']
  },
  'Manawatu': {
    'Horowhenua': ['Foxton', 'Foxton Beach', 'Heatherlea', 'Hokio Beach', 'Ihakara', 'Kuku', 'Levin', 'Manakau', 'Muhunoa East', 'Ohau', 'Shannon', 'Tokomaru', 'Waitarere'],
    'Manawatu': ['Bunnythorpe', 'Fielding', 'Highbury', 'Himatangi', 'Hiwinui', 'Kelvin Grove', 'Newbury', 'Rongotea', 'Sanson', 'Tangimoana'],
    'Palmerston North': ['Aokautere', 'Ashhurst', 'Awapuni', 'City Centre', 'Cloverlea', 'Fitzherbert', 'Highbury', 'Hokowhitu', 'Kelvin Grove', 'Linton', 'Milson', 'Papaioea', 'Roslyn', 'Takaro', 'Terrace End', 'Tiritea', 'West End', 'Westbrook'],
    'Rangitikei': ['Bulls', 'Hunterville', 'Marton', 'Taihape', 'Turakina'],
    'Ruapehu': ['National Park', 'Ohakune', 'Owhango', 'Raetihi', 'Taumaranui', 'Waiouru'],
    'Tararua': ['Dannevirke', 'Eketahuna', 'Pahiatua', 'Woodville'],
    'Wanganui': ['Aramoho', 'Bastia Hill', 'Castlecliff', 'City Centre', 'College Estate', 'Durie Hill', 'Gonville', 'Kai Iwi', 'Kaitoke', 'Marybank', 'Maxwell', 'Mosston', 'Okoia', 'Otamatea', 'Rapanui', 'Saint Johns Hill', 'Springvale', 'Tawhero', 'Upokongaro', 'Wanganui East', 'Westmere']
  },
  'Wellington': {
    'Carterton': ['Carterton'],
    'Kapiti Coast': ['Otaihanga', 'Otaki', 'Otaki Beach', 'Paekakariki', 'Paraparaumu', 'Paraparaumu Beach', 'Peka Peka', 'Raumati Beach', 'Raumati South', 'Te Horo', 'Waikanae', 'Waikanae Beach'],
    'Lower Hutt City': [
      'Alicetown', 'Avalon', 'Belmont', 'Boulcott', 'Days Bay', 'Eastbourne', 'Epuni', 'Fairfield', 'Gracefield',
      'Harbour View', 'Holborn', 'Kelson', 'Korokoro', 'Lower Hutt', 'Manor Park', 'Maungaraki', 'Moera', 'Muritai',
      'Naenae', 'Normandale', 'Petone', 'Rona Bay', 'Stokes Valley', 'Taita', 'Tirohanga', 'Wainuiomata', 'Waiwhetu',
      'Waterloo', 'Woburn'
    ],
    'Masterton': ['Masterton', 'Riversdale Beach'],
    'Porirua City': ['Aotea', 'Ascot Park', 'Camborne', 'Cannons Creek', 'Elsdon', 'Karehana Bay', 'Linden', 'Mana', 'Papakowhai', 'Paremata', 'Pautahanui', 'Plimmerton', 'Porirua', 'Porirua East', 'Pukerua Bay', 'Ranui Heights', 'Takapuwahia', 'Titahi Bay', 'Waitangirua', 'Whitby'],
    'South Wairarapa': ['Featherston', 'Greytown', 'Martinborough'],
    'Upper Hutt City': ['Akatarawa', 'Birchville', 'Clouston Park', 'Elderslea', 'Heretaunga', 'Mangaroa', 'Maoribank', 'Pinehaven', 'Riverstone Terraces', 'Silverstream', 'Te Marua', 'The Plateau', 'Timberlea', 'Totara Park', 'Trentham', 'Upper Hutt', 'Wallaceville', 'Whitemans Valley'],
    'Wellington City': [
      'Aro Valley', 'Berhampore', 'Breaker Bay', 'Broadmeadows', 'Brooklyn', 'Chartwell', 'Churton Park', 'Crawford',
      'Crofton Downs', 'Evans Bay', 'Glenside', 'Grenada', 'Hataitai', 'Highbury', 'Horokiwi', 'Houghton Bay',
      'Island Bay', 'Johnsonville', 'Kaiwharawhara', 'Karaka Bays', 'Karori', 'Kelburn', 'Khandallah', 'Kilbirnie',
      'Kingston', 'Kowhai Park', 'Lambton', 'Linden', 'Lyall Bay', 'Makara - Oharlu', 'Maupuia', 'Melrose', 'Miramar',
      'Moa Point', 'Mornington', 'Mount Cook', 'Mount Victoria', 'Newlands', 'Newtown', 'Ngaio', 'Northland',
      'Oriental Bay', 'Owhiro Bay', 'Paparangi', 'Pipitea', 'Raroa', 'Redwood', 'Roseneath', 'Seatoun',
      'Strathmore Park', 'Tawa', 'Te Aro', 'Thorndon', 'Vogeltown', 'Wadestown', 'Wellington Central', 'Wilton',
      'Woodridge'
    ]
  },
  'Nelson / Tasman': {
    'Nelson City': ['Annesbrook', 'Atawhai', 'Bishopdale', 'Britannia Heights', 'Brooklands', 'City Centre', 'Maitai', 'Maitlands', 'Marybank', 'Moana', 'Nayland', 'Nelson East', 'Stepneyville', 'Stoke', 'Tahunanui', 'The Brook', 'The Wood', 'Wakatu'],
    'Tasman': ['Brightwater', 'Golden Bay', 'Mapua', 'Motueka', 'Murchison', 'Richmond', 'Riwaka', 'Saint Arnaud', 'Takaka', 'Upper Moutere', 'Wakefield']
  },
  'Marlborough': {
    'Kaikoura': ['Kaikoura'],
    'Marlborough': [
      'Blenhiem', 'Dashwood', 'Fairhall', 'Grovetown', 'Havelock', 'Kenepuru Sounds', 'Linkwater', 'Marlborough Sounds',
      'Pelorus Sounds', 'Picton', 'Portage', 'Queen Charlotte Sounds', 'Rapaura', 'Redwoodtown', 'Renwick',
      'Riverlands', 'Riversdale', 'Seddon', 'Springcreek', 'Springlands', 'Tennyson Inlet', 'Taumarina',
      'Waihopai Valley', 'Waikawa', 'Wairau Valley', 'Witherlea', 'Woodbourne'
    ]
  },
  'West Coast': {
    'Buller': ['Carters Beach', 'Charleston', 'Fairdown', 'Granity', 'Hector', 'Karamea', 'Punakaiki', 'Reefton', 'Seddonville', 'Waimangaroa', 'Westport'],
    'Grey': ['Ahaura', 'Barrytown', 'Blackball', 'Blaketown', 'Cobden', 'Dobson', 'Dunollie', 'Greymouth', 'Ikamatua', 'Karoro', 'Lake Brunner', 'Moana', 'Nelson Creek', 'Ngahere', 'Paroa', 'Rapahoe', 'Runanga', 'Rutherglen', 'South Beach', 'Taylorville', 'Totara Flat'],
    'Westland': ['Bruce Bay', 'Camerons', 'Fox Glacier', 'Franz Josef', 'Haast', 'Harihari', 'Hokitika', 'Kaniere', 'Kokatahi', 'Kowhitirangi', 'Kumara', 'Okuru', 'Ross']
  },
  'Canterbury': {
    'Ashburton': ['Ashburton', 'Fairtown', 'Hinds', 'Methven', 'Rakaia', 'Tinwald'],
    'Banks Peninsula': ['Akaroa', 'Diamond Harbour', 'Duvauchelle', 'Governors Bay', 'Little River', 'Lyttelton', 'Wainui'],
    'Christchurch City': [
      'Addington', 'Aranui', 'Avondale', 'Avonhead', 'Avonside', 'Balmoral Hill', 'Beckenham', 'Belfast', 'Bexley',
      'Bishopdale', 'Bromley', 'Broomfield', 'Bryndwr', 'Burnside', 'Burwood', 'Casebrook', 'Cashmere', 'Chaneys',
      'City Centre', 'Clifton', 'Dallington', 'Fendalton', 'Ferrymead', 'Halswell', 'Harewood', 'Heathcote',
      'Hei Hei', 'Hillmorton', 'Hillsborough', 'Hoon Hay', 'Hornby', 'Huntsbury', 'Hyde Park', 'Ilam', 'Islington',
      'Kainga', 'Linwood', 'Mairehau', 'Marshland', 'Merivale', 'Middleton', 'Moncks Bay', 'Mount Pleasant',
      'Murray Aynsley', 'New Brighton', 'North Linwood', 'North New Brighton', 'Oaklands', 'Opawa', 'Papanui',
      'Parklands', 'Phillipstown', 'Redcliffs', 'Redwood', 'Riccarton', 'Richmond', 'Russley', 'Scarborough',
      'Shirley', 'Sockburn', 'Somerfield', 'South New Brighton', 'Southshore', 'Spencerville', 'Spreydon',
      'St. Albans', 'St. Andrews Hill', 'St. Martins', 'Styx', 'Sumner', 'Sydenham', 'Templeton', 'Upper Riccarton',
      'Waimairi Beach', 'Wainoni', 'Waltham', 'Westlake', 'Westmorland', 'Wigram', 'Wigram Park', 'Woolston'
    ],
    'Hurunui': ['Amberley', 'Amberley Beach', 'Cheviot', 'Culverden', 'Hanmer Springs', 'Hawarden', 'Leithfield', 'Leithfield Beach', 'Motunau', 'Waiau', 'Waikari', 'Waipara'],
    'Mackenzie': ['Fairlie', 'Lake Tekapo', 'Twizel'],
    'Selwyn': ['Arthurs Pass', 'Burnham', 'Coalgate', 'Darfield', 'Dunsandel', 'Hororata', 'Kirwee', 'Lake Ellesmere', 'Leeston', 'Lincoln', 'Prebbleton', 'Rolleston', 'Sheffield', 'Southbridge', 'Springfield', 'Springston', 'Tai Tapu', 'Weedons', 'West Melton'],
    'Timaru': ['Arundel', 'Geraldine', 'Pareora', 'Pleasant Point', 'Temuka', 'Timaru', 'Washdyke'],
    'Waimakariri': ['Ashley', 'Bexley', 'Carleton', 'Clarkville', 'Coldstream', 'Cust', 'Fernside', 'Flaxton', 'Horrellville', 'Kaiapoi', 'Loburn North', 'Ohapuku', 'Ohoka', 'Oxford', 'Pegasus Bay', 'Rangiora', 'Sefton', 'Southbrook', 'Summerhill', 'Swannanoa', 'Waikuku', 'West Eyerton', 'White Rock', 'Woodend'],
    'Waimate': ['Saint Andrews', 'Waimate']
  },
  'Otago': {
    'Central Otago': ['Alexandra', 'Clyde', 'Cromwell', 'Ettrick', 'Naseby', 'Omakau', 'Oturehau', 'Ranfurly', 'Roxburgh', 'Tarras'],
    'Clutha': ['Balclutha', 'Clinton', 'Kaitangata', 'Kaka Point', 'Lawrence', 'Milton', 'Owaka', 'Tapanui', 'Taumata', 'Waihola', 'Wairuna'],
    'Dunedin City': [
      'Abbotsford', 'Allanton', 'Andersons Bay', 'Balaclava', 'Belleknowes', 'Bradford', 'Brighton', 'Broad Bay',
      'Brockville', 'Calton Hill', 'Caversham', 'City Centre', 'Clyde Hill', 'Company Bay', 'Concord', 'Corstorphine',
      'Dalmore', 'East Taieri', 'Fairfield', 'Glenleith', 'Glenross', 'Green Island', 'Halfway Bush', 'Harwood',
      'Helensburgh', 'Heyward Point', 'Highcliff', 'Kaikorai', 'Kaikorai Valley', 'Karitane', 'Kenmure', 'Kensington',
      'Kew', 'Kinmont Park', 'Leith Valley', 'Liberton', 'Littlebourne', 'Macandrew Bay', 'Maori Hill', 'Maryhill',
      'Maungatua', 'Mornington', 'Mosgiel', 'Mount Mera', 'Musselburgh', 'Normanby', 'North Dunedin',
      'North East Valley', 'Ocean View', 'Opoho', 'Otago Peninsula', 'Otokia', 'Outram', 'Pine Hill', 'Port Chalmers',
      'Portobello', 'Ravensbourne', 'Roseneath', 'Roslyn', 'Saint Clair', 'Saint Clair Park', 'Saint Kilda',
      'Saint Leonards', 'Sawyers Bay', 'Scroggs Hill', 'Shiel Hill', 'South Dunedin', 'Tainui', 'Waikouaiti',
      'Waitati', 'Wakari', 'Waldronville', 'Warrington', 'Waverley', 'Woodhaugh'
    ],
    'Queenstown-Lakes': ['Albert Town', 'Arrow Junction', 'Arrowtown', 'Arthurs Point', 'Boydtown', 'Cardrona', 'Closeburn', 'Crown Terrace', 'Dalefield', 'Fernhill', 'Frankton', 'Gibbston', 'Glenorchy', 'Goldfield Heights', 'Kelvin Peninsula', 'Kingston', 'Lake Hawea', 'Lake Hayes', 'Lake Wakatipu', 'Lower Shotover', 'Luggate', 'Makarora', 'Queenstown East', 'Queenstown Hill', 'Sunshine Bay', 'Town Centre', 'Wanaka'],
    'Waitaki': ['Kurow', 'Lake Ohau', 'Moeraki', 'Oamaru', 'Omarama', 'Otematata', 'Palmerston', 'Sailors Cutting', 'South Oamaru', 'Taranui', 'Twizel', 'Waiareka', 'Weston']
  },
  'Southland': {
    'Gore': ['Gore', 'Mataura'],
    'Invercargill City': ['Appleby', 'Avenal', 'Bluff', 'City Centre', 'Clifton', 'Georgetown', 'Gladstone', 'Glengarry', 'Grasmere', 'Hawthorndale', 'Heidelberg', 'Kennington', 'Kew', 'Kingswell', 'Makarewa', 'Myross Bush', 'Newfield', 'Otatara', 'Richmond', 'Rosedale', 'Strathem', 'Tisbury', 'Waikiwi', 'Windsor'],
    'Southland': ['Browns', 'Colac Bay', 'Dipton West', 'Drummond', 'Edendale', 'Limehills', 'Lumsden', 'Manapouri', 'Mossburn', 'Nightcaps', 'Oban', 'Ohai', 'Orepuki', 'Otautau', 'Riversdale', 'Riverton', 'Ryal Bush', 'Stewart Island', 'Te Anau', 'Thornbury', 'Tokanui', 'Tuatapere', 'Waikaia', 'Wallacetown', 'Waverley', 'Winton', 'Woodlands', 'Wyndham']
  }
};

export const nzLocations: NzLocation[] = Object.entries(nzRegionDistrictSuburbs)
  .map(([region, districts]) => {
    const cities = Array.from(
      new Set(
        Object.keys(districts)
          .map((city) => city.trim())
          .filter(Boolean)
      )
    ).sort((a, b) => a.localeCompare(b));

    return { region, cities };
  })
  .sort((a, b) => a.region.localeCompare(b.region));
