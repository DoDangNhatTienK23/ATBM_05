-- ============================================================
-- OLS_SYS.sql
-- Chay bang: SYS AS SYSDBA
-- Thu tu: Chay sau hospital_setup.sql, truoc OLS_BVDBA.sql
-- ============================================================
-- Noi dung:
--   Buoc 1: Kich hoat OLS cho schema BVDBA
--   Buoc 2: Tao policy BENHVIEN_POL, gan vao bang THONGBAO
--   Buoc 3: Tao Level, Compartment, Group
--   Buoc 4: Tao nhan du lieu (t1~t7)
--   Buoc 6: Gan nhan cho nguoi dung (u1~u8)
-- ============================================================
 
 
-- ============================================================
-- BUOC 1: KICH HOAT OLS CHO SCHEMA BVDBA
-- ============================================================
 
EXECUTE LBACSYS.SA_SYSDBA.ENABLE_SCHEMA(schema_name => 'BVDBA', option_list => '');
 
 
-- ============================================================
-- BUOC 2: TAO CHINH SACH OLS
-- ============================================================
 
-- Xoa chinh sach cu neu ton tai
BEGIN
    LBACSYS.SA_SYSDBA.DROP_POLICY(policy_name => 'BENHVIEN_POL');
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
 
-- Tao chinh sach moi
BEGIN
    LBACSYS.SA_SYSDBA.CREATE_POLICY(
        policy_name     => 'BENHVIEN_POL',
        column_name     => 'OLS_LABEL',
        default_options => 'READ_CONTROL,WRITE_CONTROL,LABEL_DEFAULT'
    );
END;
/
 
-- Gan chinh sach vao bang THONGBAO cua BVDBA
BEGIN
    LBACSYS.SA_TABLE.SET_PROTECTION(
        policy_name   => 'BENHVIEN_POL',
        schema_name   => 'BVDBA',
        table_name    => 'THONGBAO',
        table_options => 'READ_CONTROL,WRITE_CONTROL',
        label_column  => 'OLS_LABEL'
    );
END;
/
 
 
-- ============================================================
-- BUOC 3: TAO CAC THANH PHAN NHAN
-- ============================================================
 
-- ---- 3.1: LEVELS (Cap bac) ----
-- BGD(30) > LDK(20) > NV(10)
-- User co level cao hon se doc duoc du lieu co level thap hon
BEGIN
    LBACSYS.SA_COMPONENTS.CREATE_LEVEL(
        policy_name => 'BENHVIEN_POL',
        level_num   => 30,
        short_name  => 'BGD',
        long_name   => 'Ban Giam Doc'
    );
END;
/
BEGIN
    LBACSYS.SA_COMPONENTS.CREATE_LEVEL(
        policy_name => 'BENHVIEN_POL',
        level_num   => 20,
        short_name  => 'LDK',
        long_name   => 'Lanh Dao Khoa'
    );
END;
/
BEGIN
    LBACSYS.SA_COMPONENTS.CREATE_LEVEL(
        policy_name => 'BENHVIEN_POL',
        level_num   => 10,
        short_name  => 'NV',
        long_name   => 'Nhan Vien'
    );
END;
/
 
-- ---- 3.2: COMPARTMENTS (Khoa chuyen mon) ----
BEGIN
    LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT(
        policy_name => 'BENHVIEN_POL',
        comp_num    => 1,
        short_name  => 'TH',
        long_name   => 'Khoa Tieu Hoa'
    );
END;
/
BEGIN
    LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT(
        policy_name => 'BENHVIEN_POL',
        comp_num    => 2,
        short_name  => 'TK',
        long_name   => 'Khoa Than Kinh'
    );
END;
/
BEGIN
    LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT(
        policy_name => 'BENHVIEN_POL',
        comp_num    => 3,
        short_name  => 'TM',
        long_name   => 'Khoa Tim Mach'
    );
END;
/
 
-- ---- 3.3: GROUPS (Co so) ----
BEGIN
    LBACSYS.SA_COMPONENTS.CREATE_GROUP(
        policy_name => 'BENHVIEN_POL',
        group_num   => 10,
        short_name  => 'HCM',
        long_name   => 'Ho Chi Minh'
    );
END;
/
BEGIN
    LBACSYS.SA_COMPONENTS.CREATE_GROUP(
        policy_name => 'BENHVIEN_POL',
        group_num   => 20,
        short_name  => 'HP',
        long_name   => 'Hai Phong'
    );
END;
/
BEGIN
    LBACSYS.SA_COMPONENTS.CREATE_GROUP(
        policy_name => 'BENHVIEN_POL',
        group_num   => 30,
        short_name  => 'HN',
        long_name   => 'Ha Noi'
    );
END;
/
 
 
-- ============================================================
-- BUOC 4: TAO NHAN CHO DU LIEU (t1~t7)
-- Cau truc: LEVEL:COMPARTMENT:GROUP
--   t1: NV          -> Moi nhan vien, moi khoa, moi co so
--   t2: BGD         -> Chi Ban Giam Doc
--   t3: LDK         -> Lanh dao khoa, moi khoa, moi co so
--   t4: LDK:TH      -> Lanh dao Khoa Tieu hoa, moi co so
--   t5: NV:TH:HCM   -> Nhan vien Khoa Tieu hoa tai HCM
--   t6: NV:TH:HN    -> Nhan vien Khoa Tieu hoa tai Ha Noi
--   t7: LDK:TH,TK:HP -> Lanh dao Khoa TH va TK tai Hai Phong
-- ============================================================
 
BEGIN
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL(
        policy_name => 'BENHVIEN_POL', label_tag => 1001,
        label_value => 'NV', data_label => TRUE);
END;
/
BEGIN
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL(
        policy_name => 'BENHVIEN_POL', label_tag => 1002,
        label_value => 'BGD', data_label => TRUE);
END;
/
BEGIN
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL(
        policy_name => 'BENHVIEN_POL', label_tag => 1003,
        label_value => 'LDK', data_label => TRUE);
END;
/
BEGIN
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL(
        policy_name => 'BENHVIEN_POL', label_tag => 1004,
        label_value => 'LDK:TH', data_label => TRUE);
END;
/
BEGIN
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL(
        policy_name => 'BENHVIEN_POL', label_tag => 1005,
        label_value => 'NV:TH:HCM', data_label => TRUE);
END;
/
BEGIN
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL(
        policy_name => 'BENHVIEN_POL', label_tag => 1006,
        label_value => 'NV:TH:HN', data_label => TRUE);
END;
/
BEGIN
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL(
        policy_name => 'BENHVIEN_POL', label_tag => 1007,
        label_value => 'LDK:TH,TK:HP', data_label => TRUE);
END;
/
 
 
-- ============================================================
-- BUOC 6: GAN NHAN CHO NGUOI DUNG (u1~u8)
-- Nguoi dung doc duoc du lieu khi:
--   user.max_level >= data.level
--   user.compartments INTERSECT data.compartments (neu du lieu co compartment)
--   user.groups INTERSECT data.groups (neu du lieu co group)
-- ============================================================
 
-- u1: Giam doc - doc toan bo thong bao (BGD max, moi khoa, moi co so)
BEGIN
    LBACSYS.SA_USER_ADMIN.SET_LEVELS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U1_GIAMDOC',
        max_level   => 'BGD', min_level => 'NV');
    LBACSYS.SA_USER_ADMIN.SET_COMPARTMENTS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U1_GIAMDOC',
        read_comps  => 'TH,TK,TM', write_comps => NULL);
    LBACSYS.SA_USER_ADMIN.SET_GROUPS(
        policy_name  => 'BENHVIEN_POL',
        user_name    => 'U1_GIAMDOC',
        read_groups  => 'HCM,HP,HN', write_groups => NULL);
END;
/
 
-- u2: Lanh dao Khoa Tim mach tai Ho Chi Minh
BEGIN
    LBACSYS.SA_USER_ADMIN.SET_LEVELS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U2_LDKTIMMACH_HCM',
        max_level   => 'LDK', min_level => 'NV');
    LBACSYS.SA_USER_ADMIN.SET_COMPARTMENTS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U2_LDKTIMMACH_HCM',
        read_comps  => 'TM', write_comps => NULL);
    LBACSYS.SA_USER_ADMIN.SET_GROUPS(
        policy_name  => 'BENHVIEN_POL',
        user_name    => 'U2_LDKTIMMACH_HCM',
        read_groups  => 'HCM', write_groups => NULL);
END;
/
 
-- u3: Lanh dao Khoa Than kinh tai Ha Noi
BEGIN
    LBACSYS.SA_USER_ADMIN.SET_LEVELS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U3_LDKTHANKINH_HN',
        max_level   => 'LDK', min_level => 'NV');
    LBACSYS.SA_USER_ADMIN.SET_COMPARTMENTS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U3_LDKTHANKINH_HN',
        read_comps  => 'TK', write_comps => NULL);
    LBACSYS.SA_USER_ADMIN.SET_GROUPS(
        policy_name  => 'BENHVIEN_POL',
        user_name    => 'U3_LDKTHANKINH_HN',
        read_groups  => 'HN', write_groups => NULL);
END;
/
 
-- u4: Nhan vien Khoa Than kinh tai Ho Chi Minh
BEGIN
    LBACSYS.SA_USER_ADMIN.SET_LEVELS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U4_NVTHANKINH_HCM',
        max_level   => 'NV', min_level => 'NV');
    LBACSYS.SA_USER_ADMIN.SET_COMPARTMENTS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U4_NVTHANKINH_HCM',
        read_comps  => 'TK', write_comps => NULL);
    LBACSYS.SA_USER_ADMIN.SET_GROUPS(
        policy_name  => 'BENHVIEN_POL',
        user_name    => 'U4_NVTHANKINH_HCM',
        read_groups  => 'HCM', write_groups => NULL);
END;
/
 
-- u5: Nhan vien Khoa Tim mach tai Ho Chi Minh
BEGIN
    LBACSYS.SA_USER_ADMIN.SET_LEVELS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U5_NVTIMMACH_HCM',
        max_level   => 'NV', min_level => 'NV');
    LBACSYS.SA_USER_ADMIN.SET_COMPARTMENTS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U5_NVTIMMACH_HCM',
        read_comps  => 'TM', write_comps => NULL);
    LBACSYS.SA_USER_ADMIN.SET_GROUPS(
        policy_name  => 'BENHVIEN_POL',
        user_name    => 'U5_NVTIMMACH_HCM',
        read_groups  => 'HCM', write_groups => NULL);
END;
/
 
-- u6: Lanh dao phong Khoa Tim mach tai Ho Chi Minh
BEGIN
    LBACSYS.SA_USER_ADMIN.SET_LEVELS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U6_LDPTIMMACH_HCM',
        max_level   => 'LDK', min_level => 'NV');
    LBACSYS.SA_USER_ADMIN.SET_COMPARTMENTS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U6_LDPTIMMACH_HCM',
        read_comps  => 'TM', write_comps => NULL);
    LBACSYS.SA_USER_ADMIN.SET_GROUPS(
        policy_name  => 'BENHVIEN_POL',
        user_name    => 'U6_LDPTIMMACH_HCM',
        read_groups  => 'HCM', write_groups => NULL);
END;
/
 
-- u7: Lanh dao phong - doc toan bo thong bao cap LDK, moi khoa, moi co so
BEGIN
    LBACSYS.SA_USER_ADMIN.SET_LEVELS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U7_LDPTOBO',
        max_level   => 'LDK', min_level => 'NV');
    LBACSYS.SA_USER_ADMIN.SET_COMPARTMENTS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U7_LDPTOBO',
        read_comps  => 'TH,TK,TM', write_comps => NULL);
    LBACSYS.SA_USER_ADMIN.SET_GROUPS(
        policy_name  => 'BENHVIEN_POL',
        user_name    => 'U7_LDPTOBO',
        read_groups  => 'HCM,HP,HN', write_groups => NULL);
END;
/
 
-- u8: Nhan vien Khoa Tieu hoa tai Ha Noi
BEGIN
    LBACSYS.SA_USER_ADMIN.SET_LEVELS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U8_NVTIEUHOA_HN',
        max_level   => 'NV', min_level => 'NV');
    LBACSYS.SA_USER_ADMIN.SET_COMPARTMENTS(
        policy_name => 'BENHVIEN_POL',
        user_name   => 'U8_NVTIEUHOA_HN',
        read_comps  => 'TH', write_comps => NULL);
    LBACSYS.SA_USER_ADMIN.SET_GROUPS(
        policy_name  => 'BENHVIEN_POL',
        user_name    => 'U8_NVTIEUHOA_HN',
        read_groups  => 'HN', write_groups => NULL);
END;
/