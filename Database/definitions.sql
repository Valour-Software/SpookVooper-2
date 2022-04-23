CREATE TABLE IF NOT EXISTS dataprotectionkeys (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    friendlyname text NULL,
    xml text NULL,
    CONSTRAINT pk_dataprotectionkeys PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS districts (
    id VARCHAR(36) NOT NULL,
    name VARCHAR(64) NULL,
    description VARCHAR(512) NULL,
    groupid VARCHAR(38) NOT NULL,
    senatorid VARCHAR(38) NULL,
    governorid VARCHAR(38) NULL,
    flagurl VARCHAR(128) NULL,
    CONSTRAINT pk_districts PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS elections (
    id VARCHAR(36) NOT NULL,
    districtid VARCHAR(64) NULL,
    start_date timestamp with time zone NOT NULL,
    end_date timestamp with time zone NOT NULL,
    winnerid VARCHAR(38) NULL,
    choiceids CHAR(38)[] NOT NULL,
    type integer NOT NULL,
    CONSTRAINT pk_elections PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS factories (
    id VARCHAR(36) NOT NULL,
    name VARCHAR(64) NULL,
    description VARCHAR(1024) NULL,
    ownerid VARCHAR(38) NOT NULL,
    countyid VARCHAR(36) NOT NULL,
    recipename VARCHAR(256) NULL,
    employeeid text NULL,
    quantity double precision NOT NULL,
    quantitygrowthrate double precision NOT NULL,
    quantitycap double precision NOT NULL,
    efficiency double precision NOT NULL,
    size integer NOT NULL,
    hourssincechangedproductionrecipe integer NOT NULL,
    age integer NOT NULL,
    CONSTRAINT pk_factories PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS forumposts (
    id VARCHAR(36) NOT NULL,
    authorid VARCHAR(38) NOT NULL,
    category integer NOT NULL,
    title VARCHAR(64) NOT NULL,
    content VARCHAR(32768) NOT NULL,
    tags text[] NOT NULL,
    timeposted timestamp with time zone NOT NULL,
    CONSTRAINT pk_forumposts PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS grouproles (
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


CREATE TABLE IF NOT EXISTS groups (
    id VARCHAR(38) NOT NULL,
    name VARCHAR(64) NOT NULL,
    description VARCHAR(2048) NULL,
    image_url VARCHAR(512) NULL,
    districtid VARCHAR(38) NULL,
    credits DECIMAL(20,10) NOT NULL,
    creditsnapshots numeric[] NULL,
    membersids text[] NOT NULL,
    api_key VARCHAR(36) NOT NULL,
    grouptype integer NOT NULL,
    flags integer[] NOT NULL,
    open boolean NOT NULL,
    ownerid VARCHAR(38) NOT NULL,
    CONSTRAINT pk_groups PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS taxpolicies (
    id VARCHAR(36) NOT NULL,
    name VARCHAR(64) NULL,
    rate DECIMAL(20,10) NOT NULL,
    districtid VARCHAR(38) NULL,
    taxtype integer NOT NULL,
    minimum DECIMAL(20,10) NOT NULL,
    maximum DECIMAL(20,10) NOT NULL,
    collected DECIMAL(20,10) NOT NULL,
    CONSTRAINT pk_taxpolicies PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS tradeitemdefinitions (
    id VARCHAR(36) NOT NULL,
    ownerid VARCHAR(38) NOT NULL,
    name VARCHAR(64) NOT NULL,
    description VARCHAR(1024) NULL,
    created timestamp with time zone NOT NULL,
    modifiers text NULL,
    CONSTRAINT pk_tradeitemdefinitions PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS tradeitems (
    id VARCHAR(36) NOT NULL,
    ownerid VARCHAR(38) NOT NULL,
    definition_id VARCHAR(36) NOT NULL,
    amount integer NOT NULL,
    CONSTRAINT pk_tradeitems PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS transactions (
    id VARCHAR(36) NOT NULL,
    credits DECIMAL(20,10) NOT NULL,
    time timestamp with time zone NOT NULL,
    fromid VARCHAR(38) NOT NULL,
    toid VARCHAR(38) NOT NULL,
    transactiontype integer NOT NULL,
    details VARCHAR(1024) NOT NULL,
    CONSTRAINT pk_transactions PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS ubipolicies (
    id VARCHAR(36) NOT NULL,
    rate DECIMAL(20,10) NOT NULL,
    anyone boolean NOT NULL,
    applicablerank integer NULL,
    districtid VARCHAR(38) NULL,
    CONSTRAINT pk_ubipolicies PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS users (
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
    creditsnapshots numeric[] NULL,
    rank integer NOT NULL,
    created timestamp with time zone NOT NULL,
    image_url VARCHAR(128) NULL,
    districtid VARCHAR(38) NULL,
    CONSTRAINT pk_users PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS votes (
    id VARCHAR(36) NOT NULL,
    choiceids Char(38)[] NOT NULL,
    date timestamp with time zone NOT NULL,
    electionid VARCHAR(36) NOT NULL,
    invalid boolean NOT NULL,
    userid VARCHAR(38) NOT NULL,
    CONSTRAINT pk_votes PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS counties (
    id VARCHAR(36) NOT NULL,
    name VARCHAR(64) NULL,
    description VARCHAR(512) NULL,
    population integer NOT NULL,
    districtid VARCHAR(38) NOT NULL,
    CONSTRAINT pk_counties PRIMARY KEY (id),
    CONSTRAINT fk_counties_districts_districtid FOREIGN KEY (districtid) REFERENCES districts (id) ON DELETE CASCADE
);


CREATE TABLE IF NOT EXISTS forumlike (
    id VARCHAR(36) NOT NULL,
    postid VARCHAR(36) NOT NULL,
    addedbyid VARCHAR(38) NOT NULL,
    CONSTRAINT pk_forumlike PRIMARY KEY (id),
    CONSTRAINT fk_forumlike_forumposts_postid FOREIGN KEY (postid) REFERENCES forumposts (id) ON DELETE CASCADE
);


CREATE INDEX ix_counties_districtid ON counties (districtid);


CREATE INDEX ix_forumlike_postid ON forumlike (postid);