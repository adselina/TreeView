using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TreeView
{
    public partial class Form1 : Form
    {
        private readonly string connectionString = ConfigurationManager.AppSettings.Get("connectionString").ToString();
        //const string connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres";
        public Form1()
        {
            InitializeComponent();
            add.Enabled = false;
            update.Enabled = false;
            dataGridView1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Refresh_tree();
        }
        private void Refresh_tree()
        {
            treeView1.Nodes.Clear();
            using (var con = OpenedConnection())
            {
                var cmd = new NpgsqlCommand("Select id, manufacture_name from manufacture", con);
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        TreeNode tree = new TreeNode(dr["manufacture_name"].ToString(), 1, 0);
                        tree.Tag = dr.GetInt32(0);
                        treeView1.Nodes.Add(tree);
                        Load_tech_obj(dr.GetInt32(0), tree);
                    }
                }
            }
        }

        private NpgsqlConnection OpenedConnection()
        {
            var connection = new NpgsqlConnection();
            connection.ConnectionString = connectionString;
            try
            {
                connection.Open();
            }
            catch
            {
                MessageBox.Show("Не удалось установить соединение");
                return null;
            }
            return connection;
        }

        void Load_tech_obj(int manufacture_id, TreeNode p)
        {
            using (var con = OpenedConnection())
            {
                var cmd = new NpgsqlCommand(@"Select tech_object.id, tech_object_name from tech_object 
                     join manufacture on id_manufacture = manufacture.id where id_manufacture = @manufacture_id", con);

                cmd.Parameters.AddWithValue("@manufacture_id", manufacture_id);
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        TreeNode tree = new TreeNode(dr["tech_object_name"].ToString(), 3, 2);
                        
                        Load_excavation(dr.GetInt32(0), tree);
                        tree.Tag = dr.GetInt32(0);
                        p.Nodes.Add(tree);
                    }
                }
            }
        }
        void Load_excavation(int tech_obj_id, TreeNode p)
        {
            using (var con = OpenedConnection())
            {
                var cmd = new NpgsqlCommand(@"Select excavation.id, excavation_name from excavation 
                     join tech_object on excavation.id_tech_obj = tech_object.id where excavation.id_tech_obj = @tech_obj_id", con);

                cmd.Parameters.AddWithValue("@tech_obj_id", tech_obj_id);
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        TreeNode tree = new TreeNode(dr["excavation_name"].ToString(), 5, 4);
                        tree.Tag = dr.GetInt32(0);
                        p.Nodes.Add(tree);
                    }
                }
            }
        }


        #region delete
        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "Удаление объекта";
            dataGridView1.Columns.Clear();
            if (treeView1.SelectedNode == null)
                return;
            using (var con = OpenedConnection())
            {
                switch (treeView1.SelectedNode.Level)
                {
                    case 0:
                        Delete_manufacture(treeView1.SelectedNode, con);
                        treeView1.SelectedNode.Remove();
                        return;

                    case 1:
                        Delete_tech_object(treeView1.SelectedNode, con);
                        treeView1.SelectedNode.Remove();
                        return;

                    case 2:
                        Delete_excavation((int)treeView1.SelectedNode.Tag, con);
                        treeView1.SelectedNode.Remove();
                        return;
                }
            }
        }

        private void Delete_manufacture(TreeNode deletingNode, NpgsqlConnection connection)
        {
            //foreach (TreeNode tree in deletingNode.Nodes)
            //{ 
            //    using (var con1 = OpenedConnection())
            //        Delete_tech_object(tree, con1);
            //}
            var cmd = new NpgsqlCommand("delete from manufacture where ID = @manufactureID", connection);
            cmd.Parameters.AddWithValue("@manufactureID", (int)deletingNode.Tag);
            cmd.ExecuteNonQuery();


        }
        private void Delete_tech_object(TreeNode deletingNode, NpgsqlConnection connection)
        {
            //foreach (TreeNode tree in deletingNode.Nodes)
            //{
            //    using (var con1 = OpenedConnection())
            //        Delete_excavation((int)tree.Tag, con1);
            //}
            var cmd = new NpgsqlCommand("delete from tech_object where ID = @tech_objectID", connection);
            cmd.Parameters.AddWithValue("@tech_objectID", (int)deletingNode.Tag);
            cmd.ExecuteNonQuery();
        }
        private void Delete_excavation(int deletingNode, NpgsqlConnection connection)
        {
            var cmd = new NpgsqlCommand("delete from excavation where ID = @excavationID", connection);
            cmd.Parameters.AddWithValue("@excavationID", deletingNode);
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region add
        bool addManuf = false;
        private void добавитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "Добавление объекта";
            dataGridView1.Enabled = true;
            _update = false;
            update.Enabled = false;
            Selected_node = treeView1.SelectedNode;
            addManuf = false;
            add.Enabled = true;
            dataGridView1.Columns.Clear();
            if (treeView1.SelectedNode == null)
                return;

            using (var con = OpenedConnection())
            {
                switch (treeView1.SelectedNode.Level)
                {
                    case 0: tech_objectTable(); richTextBox2.Text = "Новый технологический объект"; break;
                    case 1: excavationTable(); richTextBox2.Text = "Новая выработка"; break;
                }
                dataGridView1.Rows.Add();
                dataGridView1.Show();
            }
        }
        private void добавитьВToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "Добавление объекта";
            dataGridView1.Enabled = true;
            _update = false;
            update.Enabled = false;
            Selected_node = treeView1.SelectedNode;
            add.Enabled = true;
            dataGridView1.Columns.Clear();
            if (treeView1.SelectedNode == null)
                return;
            richTextBox2.Text = "Новое производство";
            manufactureTable();
            dataGridView1.Rows.Add();
            dataGridView1.Show();
            addManuf = true;

        }
        
        private void Add_excavation(NpgsqlConnection connection)
        {
            var cmd = new NpgsqlCommand("Insert into excavation (id, excavation_name, exc_length, exc_diameter, id_tech_obj) " +
                "values(@id, @excavation_name, @exc_length, @exc_diameter, @id_tech_obj)", connection);
            cmd.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Integer, dataGridView1.Rows[0].Cells[0].Value);
            cmd.Parameters.AddWithValue("@excavation_name", NpgsqlTypes.NpgsqlDbType.Varchar, dataGridView1.Rows[0].Cells[1].Value);
            cmd.Parameters.AddWithValue("@exc_length", NpgsqlTypes.NpgsqlDbType.Numeric, dataGridView1.Rows[0].Cells[2].Value);
            cmd.Parameters.AddWithValue("@exc_diameter", NpgsqlTypes.NpgsqlDbType.Numeric, dataGridView1.Rows[0].Cells[3].Value);
            cmd.Parameters.AddWithValue("@id_tech_obj", NpgsqlTypes.NpgsqlDbType.Integer, (int)treeView1.SelectedNode.Tag);
            try 
            { 
                cmd.ExecuteNonQuery();
                richTextBox1.Text = $"Новая запись успешно добавлена в таблицу {treeView1.SelectedNode.Text}.";
                Refresh_tree();
                dataGridView1.Columns.Clear();
            }
            catch
            {
                richTextBox1.Text = "Введите уникальное значение id\n";
                add.Enabled = true;
            }
           
        }
        private void Add_tech_object(NpgsqlConnection connection)
        {
            var cmd = new NpgsqlCommand("Insert into tech_object (id, tech_object_name, square, id_manufacture) " +
                       "values (@tech_objectID, @tech_object_name, @tech_object_square, @id_manufacture)", connection);
            cmd.Parameters.AddWithValue("@tech_objectID", NpgsqlTypes.NpgsqlDbType.Integer, dataGridView1.Rows[0].Cells[0].Value);
            cmd.Parameters.AddWithValue("@tech_object_name", NpgsqlTypes.NpgsqlDbType.Varchar, dataGridView1.Rows[0].Cells[1].Value);
            cmd.Parameters.AddWithValue("@tech_object_square", NpgsqlTypes.NpgsqlDbType.Integer, dataGridView1.Rows[0].Cells[2].Value);
            cmd.Parameters.AddWithValue("@id_manufacture", NpgsqlTypes.NpgsqlDbType.Integer, (int)treeView1.SelectedNode.Tag);
            try
            {
                cmd.ExecuteNonQuery();
                richTextBox2.Text = "Новый технологический объект добавлен.";
                Refresh_tree();
                dataGridView1.Columns.Clear();
            }
            catch
            {
                richTextBox1.Text = "Введите уникальное значение id\n";
                add.Enabled = true;
            }

        }
        private void Add_manufacture(NpgsqlConnection connection)
        {
            try
            {
                var cmd = new NpgsqlCommand("Insert into manufacture (id, manufacture_name) values (@manufactureID, @manufacture_name)", connection);
                cmd.Parameters.AddWithValue("@manufactureID", NpgsqlTypes.NpgsqlDbType.Integer, dataGridView1.Rows[0].Cells[0].Value);
                cmd.Parameters.AddWithValue("@manufacture_name", NpgsqlTypes.NpgsqlDbType.Varchar, dataGridView1.Rows[0].Cells[1].Value);
                cmd.ExecuteNonQuery();
                richTextBox2.Text = "Новое производство успешно добавлено.";
                Refresh_tree();
                dataGridView1.Columns.Clear();
            }
            catch
            {
                richTextBox1.Text = "Введите уникальное значение id\n";
                add.Enabled = true;
            }
        }
        private TreeNode Selected_node = null;
        private void add_Click(object sender, EventArgs e)
        {

            if (Selected_node == null)
                return;
            using (var con = OpenedConnection())
            {
                
                if (addManuf)
                {
                    Add_manufacture(con);
                    addManuf = true;
                    return;
                }
                else
                {
                    switch (Selected_node.Level)
                    {
                        case 0:
                            Add_tech_object(con);
                            break;
                        case 1:
                            Add_excavation(con);
                            break;
                    }
                }
            }
        }
        #endregion
       
        
        bool _update = false;
        
        //Показ объектов
        private void tech_objectTable()
        {

            dataGridView1.Rows.Clear();
            List<string> names = new List<string>();
            List<Type> types = new List<Type>();
            if (_update)
            {
                names = new List<string> {"tech object name", "tech object square", "manufacture id" };
                types = new List<Type> { typeof(string), typeof(double), typeof(int) };
            }
            else
            {
                names = new List<string> { "id","tech object name", "tech object square"};
                types = new List<Type> { typeof(int), typeof(string), typeof(double)};
            }
            for (int i =0; i<names.Count(); i++)
            {
                dataGridView1.Columns.Add(names[i], names[i]);
                dataGridView1.Columns[i].ValueType = types[i];
            }
            
        }
        private void excavationTable()
        {
            dataGridView1.Rows.Clear();
            List<string> names = new List<string>();
            List<Type> types = new List<Type>();
            if (_update)
            {
                names = new List<string> {"excavation name", "excavation length", "excavation diameter", "tech object id" };
                types = new List<Type> { typeof(string), typeof(double), typeof(double), typeof(int) };
            }
            else
            {
                names = new List<string> { "excavation id", "excavation name", "excavation length", "excavation diameter" };
                types = new List<Type> { typeof(int), typeof(string), typeof(double), typeof(double) };
            }
            for (int i = 0; i < names.Count(); i++)
            {
                dataGridView1.Columns.Add(names[i], names[i]);
                dataGridView1.Columns[i].ValueType = types[i];
            }
        }
        private void manufactureTable()
        {
            dataGridView1.Rows.Clear();
            List<string> names = new List<string>();
            List<Type> types = new List<Type>();
            if (_update)
            {
                names = new List<string> { "manufacture name" };
                types = new List<Type> { typeof(string) };
            }
            else
            {
                names = new List<string> { "manufacture id", "manufacture name" };
                types = new List<Type> { typeof(int), typeof(string) };
            }
                for (int i = 0; i < names.Count(); i++)
            {
                dataGridView1.Columns.Add(names[i], names[i]);
                dataGridView1.Columns[i].ValueType = types[i];
            }
        }

        #region update
        private void Update_excavation(int selectedNode, NpgsqlConnection connection)
        {
            var cmd = new NpgsqlCommand("UPDATE excavation SET excavation_name = @excavation_name, " +
                "exc_length = @exc_length, exc_diameter = @exc_diameter, id_tech_obj = @id_tech where ID = @excavationID", connection);
            cmd.Parameters.AddWithValue("@excavationID", selectedNode);

            cmd.Parameters.AddWithValue("@excavation_name", NpgsqlTypes.NpgsqlDbType.Varchar, dataGridView1.Rows[0].Cells[0].Value);
            cmd.Parameters.AddWithValue("@exc_length", NpgsqlTypes.NpgsqlDbType.Numeric, dataGridView1.Rows[0].Cells[1].Value);
            cmd.Parameters.AddWithValue("@exc_diameter", NpgsqlTypes.NpgsqlDbType.Numeric, dataGridView1.Rows[0].Cells[2].Value);
            cmd.Parameters.AddWithValue("@id_tech", NpgsqlTypes.NpgsqlDbType.Integer, dataGridView1.Rows[0].Cells[3].Value);
            try
            {
                cmd.ExecuteNonQuery();
                richTextBox1.Text = $"Запись успешно обновлена {Selected_node.Text}!";
                Hide_all();
                dataGridView1.Columns.Clear();
            }
            catch
            {
                richTextBox1.Text = "Номер технического объекта указан неверно";
            }
           }
        private void Update_tech_object(int selectedNode, NpgsqlConnection connection)
        {dataGridView1.Columns.Clear();
            _update = true;
            var cmd = new NpgsqlCommand("UPDATE tech_object SET tech_object_name = @tech_object_name, " +
                "square = @tech_object_square, id_manufacture = @id_manufacture where ID = @tech_objectID", connection);
            cmd.Parameters.AddWithValue("@tech_objectID", selectedNode);


            cmd.Parameters.AddWithValue("@tech_object_name", NpgsqlTypes.NpgsqlDbType.Varchar, dataGridView1.Rows[0].Cells[0].Value);
            cmd.Parameters.AddWithValue("@tech_object_square", NpgsqlTypes.NpgsqlDbType.Numeric, dataGridView1.Rows[0].Cells[1].Value);
            cmd.Parameters.AddWithValue("@id_manufacture", NpgsqlTypes.NpgsqlDbType.Integer, dataGridView1.Rows[0].Cells[2].Value);
            try
            {
                cmd.ExecuteNonQuery();
                richTextBox1.Text = $"Запись успешно обновлена {Selected_node.Text}!";
                Hide_all();
                dataGridView1.Columns.Clear();

            }
            catch
            {
                richTextBox1.Text = "Номер производства указан неверно";
            }

        }  
        private void Update_manufacture(int selectedNode, NpgsqlConnection connection)
        {
            
            var cmd = new NpgsqlCommand("UPDATE manufacture SET manufacture_name = @manufacture_name where ID = @manufactureID", connection);
            cmd.Parameters.AddWithValue("@manufactureID", selectedNode);
            cmd.Parameters.AddWithValue("@manufacture_name", NpgsqlTypes.NpgsqlDbType.Varchar, dataGridView1.Rows[0].Cells[0].Value);
            try
            {
                cmd.ExecuteNonQuery();
                richTextBox1.Text = $"Запись успешно обновлена {Selected_node.Text}!";
                Hide_all();
                dataGridView1.Columns.Clear();
            }
            catch (Exception e)
            {
                richTextBox1.Text = e.Message;
            }
        }

        private void обновитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Enabled = true;
            Selected_node = treeView1.SelectedNode;
            int id = (int)Selected_node.Tag;
            add.Enabled = false;
            update.Enabled = true;
            _update = true;
            dataGridView1.Columns.Clear();
            if (Selected_node == null)
                return;
            richTextBox1.Text = $"Изменяйте значения в таблице";
            
            using (var con = OpenedConnection())
            {
                var cmd = new NpgsqlCommand();
                switch (Selected_node.Level)
                {
                    case 0: cmd = Select_manufacture(id, con); richTextBox2.Text = "Обновление производства"; break;
                    case 1: cmd = Select_tech_object(id, con); richTextBox2.Text = "Обновление технологического объекта"; break;
                    case 2: cmd = Select_excavation(id, con); richTextBox2.Text = "Обновление выработки"; break;
                }

                Show(cmd);
                dataGridView1.Show();
            }
        }
        private void update_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;
            using (var con = OpenedConnection())
            {

                switch (treeView1.SelectedNode.Level)
                {
                    case 0:
                        Update_manufacture((int)treeView1.SelectedNode.Tag, con);
                        break;
                    case 1:
                        Update_tech_object((int)treeView1.SelectedNode.Tag, con);
                        break;
                    case 2:
                        Update_excavation((int)treeView1.SelectedNode.Tag, con);
                        break;
                }
            }
            _update = false;
            
        }
        #endregion

        public DataTable dataTable = new DataTable();

        #region show
        private NpgsqlCommand Select_excavation(int selectedNode, NpgsqlConnection connection)
        {
            excavationTable();
            var cmd = new NpgsqlCommand();
            if (_update)
            {
                cmd = new NpgsqlCommand("Select excavation_name, exc_length, exc_diameter, id_tech_obj from excavation where ID = @excavationID", connection);
            }
            else
            {
                cmd = new NpgsqlCommand("Select id, excavation_name, exc_length, exc_diameter from excavation where ID = @excavationID", connection);
            }
            cmd.Parameters.AddWithValue("@excavationID", selectedNode);
            return cmd;     
        }
        private NpgsqlCommand Select_tech_object(int selectedNode, NpgsqlConnection connection)
        {
            tech_objectTable();
            var cmd = new NpgsqlCommand();
            if (_update)
            {
                cmd = new NpgsqlCommand("Select tech_object_name, square, id_manufacture from tech_object where ID = @tech_objectID", connection);
            }
            else 
            { 
                cmd = new NpgsqlCommand("Select id, tech_object_name, square from tech_object where ID = @tech_objectID", connection); 
            }
            
            cmd.Parameters.AddWithValue("@tech_objectID", selectedNode);
            return cmd;
        }
        private NpgsqlCommand Select_manufacture(int selectedNode, NpgsqlConnection connection)
        {
            manufactureTable();
            var cmd = new NpgsqlCommand();
            if (_update)
            {
                cmd = new NpgsqlCommand("Select manufacture_name from manufacture where ID = @manufactureID", connection);
            }
            else
            {
                cmd = new NpgsqlCommand("Select id, manufacture_name from manufacture where ID = @manufactureID", connection);
            }

            cmd.Parameters.AddWithValue("@manufactureID", selectedNode);
            return cmd;
        }

        private void показатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "Показ объектов";
            add.Enabled = false;
            update.Enabled = false;
            dataGridView1.Enabled = false;
            dataGridView1.Columns.Clear();
            if (treeView1.SelectedNode == null)
                return;
            
            var cmd = new NpgsqlCommand();

            using (var con = OpenedConnection())
            {
                switch (treeView1.SelectedNode.Level)
                {
                    case 0:
                        cmd = Select_manufacture((int)treeView1.SelectedNode.Tag, con);
                        break;

                    case 1:
                        cmd = Select_tech_object((int)treeView1.SelectedNode.Tag, con);
                        break;

                    case 2:
                        cmd = Select_excavation((int)treeView1.SelectedNode.Tag, con);
                        break;
                }
                Show(cmd);
                dataGridView1.Show();
            }
        }
        private void Show(NpgsqlCommand cmd) 
        {
            dataGridView1.Rows.Add();
            using (var dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                        dataGridView1.Rows[0].Cells[i].Value = dr[i];
                }
            }
        }
        #endregion

        #region errors and messages
        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            richTextBox1.Text = "Введен неверный формат данных!\n";
            if(dataGridView1.SelectedCells[0].ValueType.ToString() == "System.Int32")
            {
                richTextBox1.Text += "Ожидается число целого типа :)";
            }
            if (dataGridView1.SelectedCells[0].ValueType.ToString() == "System.Double")
            {
                richTextBox1.Text += "Используйте запятую вместо точки :)";
            }
        }
        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            richTextBox1.Text = "";
        }
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;
                treeView1.ContextMenuStrip = contextMenuStrip1;
                contextMenuStrip1.Show();
            }  
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode != null && treeView1.SelectedNode.Level == 2)
            {
                contextMenuStrip1.Items[0].Enabled = false;
            }
            else
            {
                contextMenuStrip1.Items[0].Enabled = true;
            }
        }
        #endregion

        private void Hide_all()
        {
            dataGridView1.Columns.Clear();
            add.Enabled = false;
            update.Enabled = false;
            Refresh_tree();
        }
    }
}

