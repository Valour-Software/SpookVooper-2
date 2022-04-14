CREATE TABLE districts (
    id VARCHAR(36) NOT NULL,
    name VARCHAR(64) NULL,
    description VARCHAR(512) NULL,
    groupid VARCHAR(38) NOT NULL,
    senator_id VARCHAR(38) NULL,
    CONSTRAINT pk_districts PRIMARY KEY (id)
);


CREATE TABLE forumposts (
    id VARCHAR(36) NOT NULL,
    authorid VARCHAR(38) NOT NULL,
    category integer NOT NULL,
    title VARCHAR(64) NOT NULL,
    content VARCHAR(32768) NOT NULL,
    tags text[] NOT NULL,
    timeposted timestamp with time zone NOT NULL,
    CONSTRAINT pk_forumposts PRIMARY KEY (id)
);


CREATE TABLE grouproles (
    id VARCHAR(36) NOT NULL,
    name VARCHAR(64) NOT NULL,
    permissionvalue numeric(20,0) NOT NULL,
    members text[] NOT NULL,
    color text NOT NULL,
    groupid VARCHAR(38) NOT NULL,
    salary DECIMAL(20,10) NOT NULL,
    authority integer NOT NULL,
    CONSTRAINT pk_grouproles PRIMARY KEY (id)
);


CREATE TABLE groups (
    id VARCHAR(38) NOT NULL,
    name VARCHAR(64) NOT NULL,
    description VARCHAR(2048) NULL,
    image_url VARCHAR(512) NULL,
    districtid VARCHAR(38) NULL,
    credits DECIMAL(20,10) NOT NULL,
    creditsyesterday DECIMAL(20,10) NOT NULL,
    api_key VARCHAR(36) NOT NULL,
    grouptype integer NOT NULL,
    flags integer[] NOT NULL,
    open boolean NOT NULL,
    ownerid VARCHAR(38) NOT NULL,
    CONSTRAINT pk_groups PRIMARY KEY (id)
);


CREATE TABLE recipes (
    id VARCHAR(36) NOT NULL,
    inputs_names text[] NOT NULL,
    inputs_amounts integer[] NOT NULL,
    output_names text[] NOT NULL,
    output_amounts integer[] NOT NULL,
    CONSTRAINT pk_recipes PRIMARY KEY (id)
);


CREATE TABLE taxpolicies (
    id VARCHAR(36) NOT NULL,
    name VARCHAR(64) NOT NULL,
    rate DECIMAL(20,10) NOT NULL,
    districtid VARCHAR(38) NULL,
    taxtype integer NOT NULL,
    minimum DECIMAL(20,10) NOT NULL,
    maximum DECIMAL(20,10) NOT NULL,
    collected DECIMAL(20,10) NOT NULL,
    CONSTRAINT pk_taxpolicies PRIMARY KEY (id)
);


CREATE TABLE tradeitemdefinitions (
    id VARCHAR(36) NOT NULL,
    ownerid VARCHAR(38) NOT NULL,
    name VARCHAR(64) NOT NULL,
    description VARCHAR(1024) NOT NULL,
    created timestamp with time zone NOT NULL,
    modifiers text NOT NULL,
    CONSTRAINT pk_tradeitemdefinitions PRIMARY KEY (id)
);


CREATE TABLE tradeitems (
    id VARCHAR(36) NOT NULL,
    ownerid VARCHAR(38) NOT NULL,
    definition_id VARCHAR(36) NOT NULL,
    amount integer NOT NULL,
    CONSTRAINT pk_tradeitems PRIMARY KEY (id)
);


CREATE TABLE transactions (
    id VARCHAR(36) NOT NULL,
    credits DECIMAL(20,10) NOT NULL,
    time timestamp with time zone NOT NULL,
    fromid VARCHAR(38) NOT NULL,
    toid VARCHAR(38) NOT NULL,
    transactiontype integer NOT NULL,
    details VARCHAR(1024) NOT NULL,
    CONSTRAINT pk_transactions PRIMARY KEY (id)
);


CREATE TABLE ubipolicies (
    id VARCHAR(36) NOT NULL,
    rate DECIMAL(20,10) NOT NULL,
    anyone boolean NOT NULL,
    applicablerank integer NULL,
    districtid VARCHAR(38) NULL,
    CONSTRAINT pk_ubipolicies PRIMARY KEY (id)
);


CREATE TABLE users (
    id VARCHAR(38) NOT NULL,
    valourid bigint NOT NULL,
    name VARCHAR(64) NOT NULL,
    description VARCHAR(1024) NULL,
    xp real NOT NULL,
    forumxp integer NOT NULL,
    messagexp real NOT NULL,
    commentlikes integer NOT NULL,
    postlikes integer NOT NULL,
    messages integer NOT NULL,
    lastsentmessage timestamp with time zone NOT NULL,
    api_key VARCHAR(36) NOT NULL,
    credits DECIMAL(20,10) NOT NULL,
    creditsyesterday DECIMAL(20,10) NOT NULL,
    rank integer NOT NULL,
    created timestamp with time zone NOT NULL,
    image_url VARCHAR(128) NULL,
    districtid VARCHAR(38) NULL,
    CONSTRAINT pk_users PRIMARY KEY (id)
);


CREATE TABLE county (
    id VARCHAR(36) NOT NULL,
    name VARCHAR(64) NULL,
    description VARCHAR(512) NULL,
    population integer NOT NULL,
    districtid VARCHAR(38) NOT NULL,
    CONSTRAINT pk_county PRIMARY KEY (id),
    CONSTRAINT fk_county_districts_districtid FOREIGN KEY (districtid) REFERENCES districts (id) ON DELETE CASCADE
);


CREATE TABLE forumlike (
    id VARCHAR(36) NOT NULL,
    postid VARCHAR(36) NOT NULL,
    addedbyid VARCHAR(38) NOT NULL,
    CONSTRAINT pk_forumlike PRIMARY KEY (id),
    CONSTRAINT fk_forumlike_forumposts_postid FOREIGN KEY (postid) REFERENCES forumposts (id) ON DELETE CASCADE
);


CREATE TABLE factories (
    id VARCHAR(36) NOT NULL,
    name VARCHAR(64) NOT NULL,
    description VARCHAR(1024) NOT NULL,
    ownerid VARCHAR(38) NOT NULL,
    efficiency integer NOT NULL,
    countyid VARCHAR(36) NOT NULL,
    recipeid VARCHAR(36) NOT NULL,
    level integer NOT NULL,
    hasanemployee boolean NOT NULL,
    damage double precision NOT NULL,
    CONSTRAINT pk_factories PRIMARY KEY (id),
    CONSTRAINT fk_factories_recipes_recipeid FOREIGN KEY (recipeid) REFERENCES recipes (id) ON DELETE CASCADE
);


CREATE INDEX ix_county_districtid ON county (districtid);


CREATE INDEX ix_factories_recipeid ON factories (recipeid);


CREATE INDEX ix_forumlike_postid ON forumlike (postid);